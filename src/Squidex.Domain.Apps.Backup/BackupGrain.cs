// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NodaTime;
using Orleans;
using Orleans.Concurrency;
using Squidex.Domain.Apps.Backup.State;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Backup
{
    [Reentrant]
    public sealed class BackupGrain : Grain, IBackupGrain
    {
        private const int MaxBackups = 10;
        private readonly IClock clock;
        private readonly IAssetStore assetStore;
        private readonly IEventDataFormatter eventDataFormatter;
        private readonly ISemanticLog log;
        private readonly IEventStore eventStore;
        private readonly IBackupArchiveLocation backupArchiveLocation;
        private readonly IStore<Guid> store;
        private CancellationTokenSource currentTask;
        private BackupStateJob currentJob;
        private Guid appId;
        private BackupState state;
        private IPersistence<BackupState> persistence;

        public BackupGrain(
            IAssetStore assetStore,
            IBackupArchiveLocation backupArchiveLocation,
            IClock clock,
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            ISemanticLog log,
            IStore<Guid> store)
        {
            Guard.NotNull(assetStore, nameof(assetStore));
            Guard.NotNull(backupArchiveLocation, nameof(backupArchiveLocation));
            Guard.NotNull(clock, nameof(clock));
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(eventDataFormatter, nameof(eventDataFormatter));
            Guard.NotNull(store, nameof(store));
            Guard.NotNull(log, nameof(log));

            this.assetStore = assetStore;
            this.backupArchiveLocation = backupArchiveLocation;
            this.clock = clock;
            this.eventStore = eventStore;
            this.eventDataFormatter = eventDataFormatter;
            this.store = store;
            this.log = log;
        }

        public override Task OnActivateAsync()
        {
            return OnActivateAsync(this.GetPrimaryKey());
        }

        public async Task OnActivateAsync(Guid appId)
        {
            this.appId = appId;

            persistence = store.WithSnapshots<BackupState, Guid>(GetType(), appId, s => state = s);

            await ReadAsync();
            await CleanupAsync();
        }

        private async Task ReadAsync()
        {
            await persistence.ReadAsync();
        }

        private async Task WriteAsync()
        {
            await persistence.WriteSnapshotAsync(state);
        }

        private async Task CleanupAsync()
        {
            var hasUpdated = false;

            foreach (var job in state.Jobs)
            {
                if (!job.Stopped.HasValue)
                {
                    await CleanupAsync(job);

                    job.Stopped = clock.GetCurrentInstant();
                    job.Failed = true;

                    hasUpdated = true;
                }
            }

            if (hasUpdated)
            {
                await WriteAsync();
            }
        }

        private async Task CleanupAsync(BackupStateJob job)
        {
            await backupArchiveLocation.DeleteArchiveAsync(job.Id);
        }

        public async Task StartNewAsync()
        {
            if (currentTask != null)
            {
                throw new DomainException("Another backup process is already running.");
            }

            if (state.Jobs.Count >= MaxBackups)
            {
                throw new DomainException($"You cannot have more than {MaxBackups} backups.");
            }

            var job = new BackupStateJob { Id = Guid.NewGuid(), Started = clock.GetCurrentInstant() };

            currentTask = new CancellationTokenSource();
            currentJob = job;

            state.Jobs.Add(job);

            await WriteAsync();

            try
            {
                using (var stream = await backupArchiveLocation.OpenStreamAsync(job.Id))
                {
                    using (var writer = new EventStreamWriter(stream))
                    {
                        await eventStore.QueryAsync(async @event =>
                        {
                            var eventData = @event.Data;

                            if (eventData.Type == nameof(AssetCreated) ||
                                eventData.Type == nameof(AssetUpdated))
                            {
                                var parsedEvent = eventDataFormatter.Parse(eventData);

                                var assetVersion = 0L;
                                var assetId = Guid.Empty;

                                if (parsedEvent.Payload is AssetCreated assetCreated)
                                {
                                    assetId = assetCreated.AssetId;
                                    assetVersion = assetCreated.FileVersion;
                                }

                                if (parsedEvent.Payload is AssetUpdated asetUpdated)
                                {
                                    assetId = asetUpdated.AssetId;
                                    assetVersion = asetUpdated.FileVersion;
                                }

                                await writer.WriteEventAsync(eventData, async attachmentStream =>
                                {
                                    await assetStore.DownloadAsync(assetId.ToString(), assetVersion, null, attachmentStream);
                                });
                            }
                            else
                            {
                                await writer.WriteEventAsync(eventData);
                            }
                        }, "AppId", appId, null, currentTask.Token);
                    }

                    stream.Position = 0;

                    currentTask.Token.ThrowIfCancellationRequested();

                    await assetStore.UploadAsync(job.Id.ToString(), 0, null, stream);

                    currentTask.Token.ThrowIfCancellationRequested();
                }
            }
            catch
            {
                job.Failed = true;
            }
            finally
            {
                job.Stopped = clock.GetCurrentInstant();

                await WriteAsync();

                currentTask = null;
                currentJob = null;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var job = state.Jobs.FirstOrDefault(x => x.Id == id);

            if (job == null)
            {
                throw new DomainObjectNotFoundException(id.ToString(), typeof(IBackupJob));
            }

            if (currentJob == job)
            {
                currentTask?.Cancel();
            }
            else
            {
                state.Jobs.Remove(job);

                await WriteAsync();
                await CleanupAsync(job);
            }
        }

        public Task<J<List<IBackupJob>>> GetStateAsync()
        {
            return Task.FromResult(new J<List<IBackupJob>>(state.Jobs.OfType<IBackupJob>().ToList()));
        }

        private bool IsRunning()
        {
            return state.Jobs.Any(x => !x.Stopped.HasValue);
        }
    }
}
