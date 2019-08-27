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
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Orleans.Concurrency;
using Squidex.Domain.Apps.Entities.Backup.Helpers;
using Squidex.Domain.Apps.Entities.Backup.State;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Orleans;
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
        private readonly IJsonSerializer serializer;
        private readonly IServiceProvider serviceProvider;
        private readonly IEventDataFormatter eventDataFormatter;
        private readonly IEventStore eventStore;
        private readonly ISemanticLog log;
        private readonly IGrainState<BackupState> state;
        private CancellationTokenSource currentTask;
        private BackupStateJob currentJob;

        public BackupGrain(
            IAssetStore assetStore,
            IBackupArchiveLocation backupArchiveLocation,
            IClock clock,
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            IJsonSerializer serializer,
            IServiceProvider serviceProvider,
            ISemanticLog log,
            IGrainState<BackupState> state)
        {
            Guard.NotNull(assetStore, nameof(assetStore));
            Guard.NotNull(backupArchiveLocation, nameof(backupArchiveLocation));
            Guard.NotNull(clock, nameof(clock));
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(eventDataFormatter, nameof(eventDataFormatter));
            Guard.NotNull(serviceProvider, nameof(serviceProvider));
            Guard.NotNull(serializer, nameof(serializer));
            Guard.NotNull(state, nameof(state));
            Guard.NotNull(log, nameof(log));

            this.assetStore = assetStore;
            this.backupArchiveLocation = backupArchiveLocation;
            this.clock = clock;
            this.eventStore = eventStore;
            this.eventDataFormatter = eventDataFormatter;
            this.serializer = serializer;
            this.serviceProvider = serviceProvider;
            this.log = log;
            this.state = state;
        }

        protected override Task OnActivateAsync(Guid key)
        {
            RecoverAfterRestartAsync().Forget();

            return TaskHelper.Done;
        }

        private async Task RecoverAfterRestartAsync()
        {
            foreach (var job in state.Value.Jobs)
            {
                if (!job.Stopped.HasValue)
                {
                    var jobId = job.Id.ToString();

                    job.Stopped = clock.GetCurrentInstant();

                    await Safe.DeleteAsync(backupArchiveLocation, jobId, log);
                    await Safe.DeleteAsync(assetStore, jobId, log);

                    job.Status = JobStatus.Failed;

                    await state.WriteAsync();
                }
            }
        }

        public async Task RunAsync()
        {
            if (currentTask != null)
            {
                throw new DomainException("Another backup process is already running.");
            }

            if (state.Value.Jobs.Count >= MaxBackups)
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

            state.Value.Jobs.Insert(0, job);

            await state.WriteAsync();

            Process(job, currentTask.Token);
        }

        private void Process(BackupStateJob job, CancellationToken ct)
        {
            ProcessAsync(job, ct).Forget();
        }

        private async Task ProcessAsync(BackupStateJob job, CancellationToken ct)
        {
            var jobId = job.Id.ToString();

            var handlers = CreateHandlers();

            var lastTimestamp = job.Started;

            try
            {
                using (var stream = await backupArchiveLocation.OpenStreamAsync(jobId))
                {
                    using (var writer = new BackupWriter(serializer, stream, true))
                    {
                        await eventStore.QueryAsync(async storedEvent =>
                        {
                            var @event = eventDataFormatter.Parse(storedEvent.Data);

                            writer.WriteEvent(storedEvent);

                            foreach (var handler in handlers)
                            {
                                await handler.BackupEventAsync(@event, Key, writer);
                            }

                            job.HandledEvents = writer.WrittenEvents;
                            job.HandledAssets = writer.WrittenAttachments;

                            lastTimestamp = await WritePeriodically(lastTimestamp);
                        }, SquidexHeaders.AppId, Key.ToString(), null, ct);

                        foreach (var handler in handlers)
                        {
                            await handler.BackupAsync(Key, writer);
                        }

                        foreach (var handler in handlers)
                        {
                            await handler.CompleteBackupAsync(Key, writer);
                        }
                    }

                    stream.Position = 0;

                    ct.ThrowIfCancellationRequested();

                    await assetStore.UploadAsync(jobId, 0, null, stream, false, ct);
                }

                job.Status = JobStatus.Completed;
            }
            catch (Exception ex)
            {
                log.LogError(ex, jobId, (ctx, w) => w
                    .WriteProperty("action", "makeBackup")
                    .WriteProperty("status", "failed")
                    .WriteProperty("backupId", ctx));

                job.Status = JobStatus.Failed;
            }
            finally
            {
                await Safe.DeleteAsync(backupArchiveLocation, jobId, log);

                job.Stopped = clock.GetCurrentInstant();

                await state.WriteAsync();

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

                await state.WriteAsync();
            }

            return lastTimestamp;
        }

        public async Task DeleteAsync(Guid id)
        {
            var job = state.Value.Jobs.FirstOrDefault(x => x.Id == id);

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
                var jobId = job.Id.ToString();

                await Safe.DeleteAsync(backupArchiveLocation, jobId, log);
                await Safe.DeleteAsync(assetStore, jobId, log);

                state.Value.Jobs.Remove(job);

                await state.WriteAsync();
            }
        }

        private IEnumerable<BackupHandler> CreateHandlers()
        {
            return serviceProvider.GetRequiredService<IEnumerable<BackupHandler>>();
        }

        public Task<J<List<IBackupJob>>> GetStateAsync()
        {
            return J.AsTask(state.Value.Jobs.OfType<IBackupJob>().ToList());
        }
    }
}
