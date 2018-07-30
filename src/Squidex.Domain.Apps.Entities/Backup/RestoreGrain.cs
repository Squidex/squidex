// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NodaTime;
using Orleans;
using Squidex.Domain.Apps.Entities.Backup.Helpers;
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
        private RefToken actor;
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
            actor = new RefToken("subject", key);

            persistence = store.WithSnapshots<RestoreState, string>(GetType(), key, s => state = s);

            await ReadAsync();

            RecoverAfterRestart();
        }

        private void RecoverAfterRestart()
        {
            RecoverAfterRestartAsync().Forget();
        }

        private async Task RecoverAfterRestartAsync()
        {
            if (state.Job?.Status == JobStatus.Started)
            {
                Log("Failed due application restart");

                await CleanupAsync();
                await WriteAsync();
            }
        }

        public Task RestoreAsync(Uri url)
        {
            Guard.NotNull(url, nameof(url));

            if (state.Job?.Status == JobStatus.Started)
            {
                throw new DomainException("A restore operation is already running.");
            }

            state.Job = new RestoreStateJob
            {
                Id = Guid.NewGuid(),
                Started = clock.GetCurrentInstant(),
                Status = JobStatus.Started,
                Uri = url
            };

            Process();

            return TaskHelper.Done;
        }

        private void Process()
        {
            ProcessAsync().Forget();
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

                    using (Profiler.Trace("Download"))
                    {
                        await DownloadAsync();
                    }

                    using (var reader = await backupArchiveLocation.OpenArchiveAsync(state.Job.Id))
                    {
                        using (Profiler.Trace("ReadEvents"))
                        {
                            await ReadEventsAsync(reader);
                        }

                        foreach (var handler in handlers)
                        {
                            using (Profiler.TraceMethod(handler.GetType(), nameof(BackupHandler.RestoreAsync)))
                            {
                                await handler.RestoreAsync(state.Job.AppId, reader);
                            }

                            Log($"Restored {handler.Name}");
                        }

                        foreach (var handler in handlers)
                        {
                            using (Profiler.TraceMethod(handler.GetType(), nameof(BackupHandler.CompleteRestoreAsync)))
                            {
                                await handler.CompleteRestoreAsync(state.Job.AppId, reader);
                            }

                            Log($"Completed {handler.Name}");
                        }
                    }

                    state.Job.Status = JobStatus.Failed;

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
                    if (ex is BackupRestoreException backupException)
                    {
                        Log(backupException.Message);
                    }
                    else
                    {
                        Log("Failed with internal error");
                    }

                    try
                    {
                        await CleanupAsync(ex);
                    }
                    catch (Exception ex2)
                    {
                        ex = ex2;
                    }

                    state.Job.Status = JobStatus.Failed;

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
                    await WriteAsync();
                }
            }
        }

        private async Task CleanupAsync(Exception exception = null)
        {
            await backupArchiveLocation.DeleteArchiveAsync(state.Job.Id);

            if (state.Job.AppId != Guid.Empty)
            {
                foreach (var handler in handlers)
                {
                    await handler.CleanupRestoreAsync(state.Job.AppId, exception);
                }

                await appCleaner.EnqueueAppAsync(state.Job.AppId);
            }
        }

        private async Task DownloadAsync()
        {
            Log("Downloading Backup");

            await backupArchiveLocation.DownloadAsync(state.Job.Uri, state.Job.Id);

            Log("Downloaded Backup");
        }

        private async Task ReadEventsAsync(BackupReader reader)
        {
            await reader.ReadEventsAsync(async (storedEvent) =>
            {
                var @event = eventDataFormatter.Parse(storedEvent.Data);

                if (@event.Payload is SquidexEvent squidexEvent)
                {
                    squidexEvent.Actor = actor;
                }
                else if (@event.Payload is AppCreated appCreated)
                {
                    state.Job.AppId = appCreated.AppId.Id;

                    await CheckCleanupStatus();
                }

                foreach (var handler in handlers)
                {
                    await handler.RestoreEventAsync(@event, state.Job.AppId, reader);
                }

                await eventStore.AppendAsync(Guid.NewGuid(), storedEvent.StreamName, new List<EventData> { storedEvent.Data });

                Log($"Read {reader.ReadEvents} events and {reader.ReadAttachments} attachments.");
            });

            Log("Reading events completed.");
        }

        private async Task CheckCleanupStatus()
        {
            var cleaner = grainFactory.GetGrain<IAppCleanerGrain>(SingleGrain.Id);

            var status = await cleaner.GetStatusAsync(state.Job.AppId);

            if (status == CleanerStatus.Cleaning)
            {
                throw new BackupRestoreException("The app is removed in the background.");
            }

            if (status == CleanerStatus.Cleaning)
            {
                throw new BackupRestoreException("The app could not be cleaned.");
            }
        }

        private void Log(string message)
        {
            state.Job.Log.Add($"{clock.GetCurrentInstant()}: {message}");
        }

        private async Task ReadAsync()
        {
            await persistence.ReadAsync();
        }

        private async Task WriteAsync()
        {
            await persistence.WriteSnapshotAsync(state);
        }

        public Task<J<IRestoreJob>> GetJobAsync()
        {
            return Task.FromResult<J<IRestoreJob>>(state.Job);
        }
    }
}
