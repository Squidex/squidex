// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using NodaTime;
using Orleans;
using Squidex.Domain.Apps.Entities.Backup.State;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public sealed class RestoreGrain : GrainOfString, IRestoreGrain
    {
        private static readonly Duration UpdateDuration = Duration.FromSeconds(1);
        private readonly IClock clock;
        private readonly IAssetStore assetStore;
        private readonly IEventDataFormatter eventDataFormatter;
        private readonly IGrainFactory grainFactory;
        private readonly IAppCleanerGrain appCleaner;
        private readonly ISemanticLog log;
        private readonly IEventStore eventStore;
        private readonly IBackupArchiveLocation backupArchiveLocation;
        private readonly IStore<string> store;
        private readonly IEnumerable<BackupHandler> handlers;
        private RestoreState state = new RestoreState();
        private IPersistence<RestoreState> persistence;

        public RestoreGrain(
            IAssetStore assetStore,
            IBackupArchiveLocation backupArchiveLocation,
            IClock clock,
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            IGrainFactory grainFactory,
            IEnumerable<BackupHandler> handlers,
            ISemanticLog log,
            IStore<string> store)
        {
            Guard.NotNull(assetStore, nameof(assetStore));
            Guard.NotNull(backupArchiveLocation, nameof(backupArchiveLocation));
            Guard.NotNull(clock, nameof(clock));
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(eventDataFormatter, nameof(eventDataFormatter));
            Guard.NotNull(grainFactory, nameof(grainFactory));
            Guard.NotNull(handlers, nameof(handlers));
            Guard.NotNull(store, nameof(store));
            Guard.NotNull(log, nameof(log));

            this.assetStore = assetStore;
            this.backupArchiveLocation = backupArchiveLocation;
            this.clock = clock;
            this.eventStore = eventStore;
            this.eventDataFormatter = eventDataFormatter;
            this.grainFactory = grainFactory;
            this.handlers = handlers;
            this.store = store;
            this.log = log;

            appCleaner = grainFactory.GetGrain<IAppCleanerGrain>(SingleGrain.Id);
        }

        public override async Task OnActivateAsync(string key)
        {
            persistence = store.WithSnapshots<RestoreState, string>(GetType(), key, s => state = s);

            await persistence.ReadAsync();

            await CleanupAsync();
        }

        public Task RestoreAsync(Uri url, RefToken user)
        {
            if (state.Job != null)
            {
                throw new DomainException("A restore operation is already running.");
            }

            state.Job = new RestoreStateJob { Started = clock.GetCurrentInstant(), Uri = url, User = user };

            return ProcessAsync();
        }

        private async Task CleanupAsync()
        {
            if (state.Job != null)
            {
                state.Job.Status = "Failed due application restart";
                state.Job.IsFailed = true;

                TryCleanup();

                await persistence.WriteSnapshotAsync(state);
            }
        }

        private async Task ProcessAsync()
        {
            using (Profiler.StartSession())
            {
                try
                {
                    log.LogInformation(w => w
                        .WriteProperty("action", "restore")
                        .WriteProperty("status", "started")
                        .WriteProperty("url", state.Job.Uri.ToString()));

                    state.Job.Status = "Downloading Backup";

                    using (Profiler.Trace("Download"))
                    {
                        await DownloadAsync();
                    }

                    state.Job.Status = "Downloaded Backup";

                    using (var stream = await backupArchiveLocation.OpenStreamAsync(state.Job.Id))
                    {
                        using (var reader = new BackupReader(stream))
                        {
                            using (Profiler.Trace("ReadEvents"))
                            {
                                await ReadEventsAsync(reader);
                            }

                            state.Job.Status = "Events read";

                            foreach (var handler in handlers)
                            {
                                using (Profiler.TraceMethod(handler.GetType(), nameof(BackupHandler.RestoreAsync)))
                                {
                                    await handler.RestoreAsync(state.Job.AppId, reader);
                                }

                                state.Job.Status = $"{handler} Processed";
                            }

                            foreach (var handler in handlers)
                            {
                                using (Profiler.TraceMethod(handler.GetType(), nameof(BackupHandler.CompleteRestoreAsync)))
                                {
                                    await handler.CompleteRestoreAsync(state.Job.AppId, reader);
                                }

                                state.Job.Status = $"{handler} Completed";
                            }
                        }
                    }

                    state.Job = null;

                    log.LogInformation(w =>
                    {
                        w.WriteProperty("action", "restore");
                        w.WriteProperty("status", "completed");
                        w.WriteProperty("url", state.Job.Uri.ToString());

                        Profiler.Session?.Write(w);
                    });
                }
                catch (Exception ex)
                {
                    state.Job.IsFailed = true;

                    if (state.Job.AppId != Guid.Empty)
                    {
                        foreach (var handler in handlers)
                        {
                            await handler.CleanupRestoreAsync(state.Job.AppId, ex);
                        }
                    }

                    TryCleanup();

                    log.LogError(ex, w =>
                    {
                        w.WriteProperty("action", "retore");
                        w.WriteProperty("status", "failed");
                        w.WriteProperty("url", state.Job.Uri.ToString());

                        Profiler.Session?.Write(w);
                    });
                }
                finally
                {
                    await persistence.WriteSnapshotAsync(state);
                }
            }
        }

        private async Task DownloadAsync()
        {
            using (var client = new HttpClient())
            {
                using (var sourceStream = await client.GetStreamAsync(state.Job.Uri.ToString()))
                {
                    using (var targetStream = await backupArchiveLocation.OpenStreamAsync(state.Job.Id))
                    {
                        await sourceStream.CopyToAsync(targetStream);
                    }
                }
            }
        }

        private async Task ReadEventsAsync(BackupReader reader)
        {
            await reader.ReadEventsAsync(async (storedEvent) =>
            {
                var eventData = storedEvent.Data;
                var eventParsed = eventDataFormatter.Parse(eventData);

                if (eventParsed.Payload is SquidexEvent squidexEvent)
                {
                    squidexEvent.Actor = state.Job.User;
                }
                else if (eventParsed.Payload is AppCreated appCreated)
                {
                    state.Job.AppId = appCreated.AppId.Id;

                    await CheckCleanupStatus();
                }

                foreach (var handler in handlers)
                {
                    await handler.RestoreEventAsync(eventParsed, state.Job.AppId, reader);
                }

                await eventStore.AppendAsync(Guid.NewGuid(), storedEvent.StreamName, new List<EventData> { storedEvent.Data });

                state.Job.Status = $"Handled event {reader.ReadEvents} events and {reader.ReadAttachments} attachments";
            });
        }

        private async Task CheckCleanupStatus()
        {
            var cleaner = grainFactory.GetGrain<IAppCleanerGrain>(SingleGrain.Id);

            var status = await cleaner.GetStatusAsync(state.Job.AppId);

            if (status == CleanerStatus.Cleaning)
            {
                throw new DomainException("The app is removed in the background.");
            }

            if (status == CleanerStatus.Cleaning)
            {
                throw new DomainException("The app could not be cleaned.");
            }
        }

        private void TryCleanup()
        {
            if (state.Job.AppId != Guid.Empty)
            {
                appCleaner.EnqueueAppAsync(state.Job.AppId).Forget();
            }
        }

        public Task<J<IRestoreJob>> GetStateAsync()
        {
            return Task.FromResult<J<IRestoreJob>>(state.Job);
        }
    }
}
