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
        private readonly IAppCleanerGrain appCleaner;
        private readonly ISemanticLog log;
        private readonly IEventStore eventStore;
        private readonly IBackupArchiveLocation backupArchiveLocation;
        private readonly IStore<string> store;
        private readonly IEnumerable<IRestoreHandler> handlers;
        private RestoreState state = new RestoreState();
        private IPersistence<RestoreState> persistence;

        public RestoreGrain(
            IAssetStore assetStore,
            IBackupArchiveLocation backupArchiveLocation,
            IClock clock,
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            IGrainFactory grainFactory,
            IEnumerable<IRestoreHandler> handlers,
            ISemanticLog log,
            IStore<Guid> store)
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
            this.handlers = handlers;
            this.store = store;
            this.log = log;
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

                if (state.Job.AppId != Guid.Empty)
                {
                    appCleaner.EnqueueAppAsync(state.Job.AppId).Forget();
                }

                await persistence.WriteSnapshotAsync(state);
            }
        }

        private async Task ProcessAsync()
        {
            try
            {
                await DoAsync(
                    "Downloading Backup",
                    "Downloaded Backup",
                    DownloadAsync);

                await DoAsync(
                    "Reading Events",
                    "Readed Events",
                    ReadEventsAsync);

                foreach (var handler in handlers)
                {
                    await DoAsync($"{handler.Name} Proessing", $"{handler.Name} Processed", handler.ProcessAsync);
                }

                foreach (var handler in handlers)
                {
                    await DoAsync($"{handler.Name} Completing", $"{handler.Name} Completed", handler.CompleteAsync);
                }

                state.Job = null;
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "makeBackup")
                    .WriteProperty("status", "failed")
                    .WriteProperty("backupId", state.Job.Id.ToString()));

                state.Job.IsFailed = true;

                if (state.Job.AppId != Guid.Empty)
                {
                    appCleaner.EnqueueAppAsync(state.Job.AppId).Forget();
                }
            }
            finally
            {
                await persistence.WriteSnapshotAsync(state);
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

        private async Task ReadEventsAsync()
        {
            using (var stream = await backupArchiveLocation.OpenStreamAsync(state.Job.Id))
            {
                using (var reader = new EventStreamReader(stream))
                {
                    var eventIndex = 0;

                    await reader.ReadEventsAsync(async (@event, attachment) =>
                    {
                        var eventData = @event.Data;

                        var parsedEvent = eventDataFormatter.Parse(eventData);

                        if (parsedEvent.Payload is SquidexEvent squidexEvent)
                        {
                            squidexEvent.Actor = state.Job.User;
                        }

                        foreach (var handler in handlers)
                        {
                            await handler.HandleAsync(parsedEvent, attachment);
                        }

                        await eventStore.AppendAsync(Guid.NewGuid(), @event.StreamName, new List<EventData> { @event.Data });

                        eventIndex++;

                        state.Job.Status = $"Handled event {eventIndex}";
                    });
                }
            }
        }

        private async Task DoAsync(string start, string end, Func<Task> action)
        {
            state.Job.Status = start;

            await action();

            state.Job.Status = end;
        }

        public Task<J<IRestoreJob>> GetStateAsync()
        {
            return Task.FromResult<J<IRestoreJob>>(state.Job);
        }
    }
}
