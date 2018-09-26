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
using Orleans.Concurrency;
using Squidex.Domain.Apps.Entities.Backup.Helpers;
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
    [Reentrant]
    public sealed class BackupGrain : GrainOfGuid, IBackupGrain
    {
        private const int MaxBackups = 10;
        private static readonly Duration UpdateDuration = Duration.FromSeconds(1);
        private readonly IAssetStore assetStore;
        private readonly IBackupArchiveLocation backupArchiveLocation;
        private readonly IClock clock;
        private readonly IEnumerable<BackupHandler> handlers;
        private readonly IEventDataFormatter eventDataFormatter;
        private readonly IEventStore eventStore;
        private readonly ISemanticLog log;
        private readonly IStore<Guid> store;
        private CancellationTokenSource currentTask;
        private BackupStateJob currentJob;
        private BackupState state = new BackupState();
        private Guid appId;
        private IPersistence<BackupState> persistence;

        public BackupGrain(
            IAssetStore assetStore,
            IBackupArchiveLocation backupArchiveLocation,
            IClock clock,
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            IEnumerable<BackupHandler> handlers,
            ISemanticLog log,
            IStore<Guid> store)
        {
            Guard.NotNull(assetStore, nameof(assetStore));
            Guard.NotNull(backupArchiveLocation, nameof(backupArchiveLocation));
            Guard.NotNull(clock, nameof(clock));
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(eventDataFormatter, nameof(eventDataFormatter));
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

        public override async Task OnActivateAsync(Guid key)
        {
            appId = key;

            persistence = store.WithSnapshots<BackupState, Guid>(GetType(), key, s => state = s);

            await ReadAsync();

            RecoverAfterRestart();
        }

        private void RecoverAfterRestart()
        {
            RecoverAfterRestartAsync().Forget();
        }

        private async Task RecoverAfterRestartAsync()
        {
            foreach (var job in state.Jobs)
            {
                if (!job.Stopped.HasValue)
                {
                    job.Stopped = clock.GetCurrentInstant();

                    await Safe.DeleteAsync(backupArchiveLocation, job.Id, log);
                    await Safe.DeleteAsync(assetStore, job.Id, log);

                    job.Status = JobStatus.Failed;

                    await WriteAsync();
                }
            }
        }

        public async Task RunAsync()
        {
            if (currentTask != null)
            {
                throw new DomainException("Another backup process is already running.");
            }

            if (state.Jobs.Count >= MaxBackups)
            {
                throw new DomainException($"You cannot have more than {MaxBackups} backups.");
            }

            var job = new BackupStateJob
            {
                Id = Guid.NewGuid(),
                Started = clock.GetCurrentInstant(),
                Status = JobStatus.Started
            };

            currentTask = new CancellationTokenSource();
            currentJob = job;

            var lastTimestamp = job.Started;

            state.Jobs.Insert(0, job);

            await WriteAsync();

            try
            {
                using (var stream = await backupArchiveLocation.OpenStreamAsync(job.Id))
                {
                    using (var writer = new BackupWriter(stream, true))
                    {
                        await eventStore.QueryAsync(async storedEvent =>
                        {
                            var @event = eventDataFormatter.Parse(storedEvent.Data);

                            writer.WriteEvent(storedEvent);

                            foreach (var handler in handlers)
                            {
                                await handler.BackupEventAsync(@event, appId, writer);
                            }

                            job.HandledEvents = writer.WrittenEvents;
                            job.HandledAssets = writer.WrittenAttachments;

                            lastTimestamp = await WritePeriodically(lastTimestamp);
                        }, SquidexHeaders.AppId, appId.ToString(), null, currentTask.Token);

                        foreach (var handler in handlers)
                        {
                            await handler.BackupAsync(appId, writer);
                        }

                        foreach (var handler in handlers)
                        {
                            await handler.CompleteBackupAsync(appId, writer);
                        }
                    }

                    stream.Position = 0;

                    currentTask.Token.ThrowIfCancellationRequested();

                    await assetStore.UploadAsync(job.Id.ToString(), 0, null, stream, currentTask.Token);
                }

                job.Status = JobStatus.Completed;
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "makeBackup")
                    .WriteProperty("status", "failed")
                    .WriteProperty("backupId", job.Id.ToString()));

                job.Status = JobStatus.Failed;
            }
            finally
            {
                await Safe.DeleteAsync(backupArchiveLocation, job.Id, log);

                job.Stopped = clock.GetCurrentInstant();

                await WriteAsync();

                currentTask = null;
                currentJob = null;
            }
        }

        private async Task<Instant> WritePeriodically(Instant lastTimestamp)
        {
            var now = clock.GetCurrentInstant();

            if ((now - lastTimestamp) >= UpdateDuration)
            {
                lastTimestamp = now;

                await WriteAsync();
            }

            return lastTimestamp;
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
                await Safe.DeleteAsync(backupArchiveLocation, job.Id, log);
                await Safe.DeleteAsync(assetStore, job.Id, log);

                state.Jobs.Remove(job);

                await WriteAsync();
            }
        }

        public Task<J<List<IBackupJob>>> GetStateAsync()
        {
            return J.AsTask(state.Jobs.OfType<IBackupJob>().ToList());
        }

        private async Task ReadAsync()
        {
            await persistence.ReadAsync();
        }

        private async Task WriteAsync()
        {
            await persistence.WriteSnapshotAsync(state);
        }
    }
}
