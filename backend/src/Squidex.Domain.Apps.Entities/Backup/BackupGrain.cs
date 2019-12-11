﻿// ==========================================================================
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
using Squidex.Domain.Apps.Entities.Backup.State;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Tasks;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Backup
{
    [Reentrant]
    public sealed class BackupGrain : GrainOfGuid, IBackupGrain
    {
        private const int MaxBackups = 10;
        private static readonly Duration UpdateDuration = Duration.FromSeconds(1);
        private readonly IBackupArchiveLocation backupArchiveLocation;
        private readonly IBackupArchiveStore backupArchiveStore;
        private readonly IClock clock;
        private readonly IServiceProvider serviceProvider;
        private readonly IEventDataFormatter eventDataFormatter;
        private readonly IEventStore eventStore;
        private readonly ISemanticLog log;
        private readonly IGrainState<BackupState> state;
        private readonly IUserResolver userResolver;
        private CancellationTokenSource? currentTaskToken;
        private BackupJob? currentJob;

        public BackupGrain(
            IBackupArchiveLocation backupArchiveLocation,
            IBackupArchiveStore backupArchiveStore,
            IClock clock,
            IEventDataFormatter eventDataFormatter,
            IEventStore eventStore,
            IGrainState<BackupState> state,
            IServiceProvider serviceProvider,
            IUserResolver userResolver,
            ISemanticLog log)
        {
            Guard.NotNull(backupArchiveLocation);
            Guard.NotNull(backupArchiveStore);
            Guard.NotNull(clock);
            Guard.NotNull(eventDataFormatter);
            Guard.NotNull(eventStore);
            Guard.NotNull(serviceProvider);
            Guard.NotNull(state);
            Guard.NotNull(userResolver);
            Guard.NotNull(log);

            this.backupArchiveLocation = backupArchiveLocation;
            this.backupArchiveStore = backupArchiveStore;
            this.clock = clock;
            this.eventDataFormatter = eventDataFormatter;
            this.eventStore = eventStore;
            this.serviceProvider = serviceProvider;
            this.state = state;
            this.userResolver = userResolver;

            this.log = log;
        }

        protected override Task OnActivateAsync(Guid key)
        {
            RecoverAfterRestartAsync().Forget();

            return TaskHelper.Done;
        }

        private async Task RecoverAfterRestartAsync()
        {
            state.Value.Jobs.RemoveAll(x => !x.Stopped.HasValue);

            await state.WriteAsync();
        }

        public async Task BackupAsync(RefToken actor)
        {
            if (currentTaskToken != null)
            {
                throw new DomainException("Another backup process is already running.");
            }

            if (state.Value.Jobs.Count >= MaxBackups)
            {
                throw new DomainException($"You cannot have more than {MaxBackups} backups.");
            }

            var job = new BackupJob
            {
                Id = Guid.NewGuid(),
                Started = clock.GetCurrentInstant(),
                Status = JobStatus.Started
            };

            currentTaskToken = new CancellationTokenSource();
            currentJob = job;

            state.Value.Jobs.Insert(0, job);

            await state.WriteAsync();

            Process(job, actor, currentTaskToken.Token);
        }

        private void Process(BackupJob job, RefToken actor, CancellationToken ct)
        {
            ProcessAsync(job, actor, ct).Forget();
        }

        private async Task ProcessAsync(BackupJob job, RefToken actor, CancellationToken ct)
        {
            var handlers = CreateHandlers();

            var lastTimestamp = job.Started;

            try
            {
                using (var stream = backupArchiveLocation.OpenStream(job.Id))
                {
                    using (var writer = await backupArchiveLocation.OpenWriterAsync(stream))
                    {
                        var userMapping = new UserMapping(actor);

                        var context = new BackupContext(Key, userMapping, writer);

                        await eventStore.QueryAsync(async storedEvent =>
                        {
                            var @event = eventDataFormatter.Parse(storedEvent.Data);

                            if (@event.Payload is SquidexEvent squidexEvent)
                            {
                                context.UserMapping.Backup(squidexEvent.Actor);
                            }

                            foreach (var handler in handlers)
                            {
                                await handler.BackupEventAsync(@event, context);
                            }

                            writer.WriteEvent(storedEvent);

                            job.HandledEvents = writer.WrittenEvents;
                            job.HandledAssets = writer.WrittenAttachments;

                            lastTimestamp = await WritePeriodically(lastTimestamp);
                        }, SquidexHeaders.AppId, Key.ToString(), null, ct);

                        foreach (var handler in handlers)
                        {
                            ct.ThrowIfCancellationRequested();

                            await handler.BackupAsync(context);
                        }

                        foreach (var handler in handlers)
                        {
                            ct.ThrowIfCancellationRequested();

                            await handler.CompleteBackupAsync(context);
                        }

                        await userMapping.StoreAsync(writer, userResolver);
                    }

                    stream.Position = 0;

                    ct.ThrowIfCancellationRequested();

                    await backupArchiveStore.UploadAsync(job.Id, stream, ct);
                }

                job.Status = JobStatus.Completed;
            }
            catch (OperationCanceledException)
            {
                await RemoveAsync(job);
            }
            catch (Exception ex)
            {
                log.LogError(ex, job.Id.ToString(), (ctx, w) => w
                    .WriteProperty("action", "makeBackup")
                    .WriteProperty("status", "failed")
                    .WriteProperty("backupId", ctx));

                job.Status = JobStatus.Failed;
            }
            finally
            {
                job.Stopped = clock.GetCurrentInstant();

                await state.WriteAsync();

                currentTaskToken = null;
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
                currentTaskToken?.Cancel();
            }
            else
            {
                await RemoveAsync(job);
            }
        }

        private async Task RemoveAsync(BackupJob job)
        {
            try
            {
                await backupArchiveStore.DeleteAsync(job.Id);
            }
            catch (Exception ex)
            {
                log.LogError(ex, job.Id.ToString(), (logOperationId, w) => w
                    .WriteProperty("action", "deleteBackup")
                    .WriteProperty("status", "failed")
                    .WriteProperty("operationId", logOperationId));
            }

            state.Value.Jobs.Remove(job);

            await state.WriteAsync();
        }

        private IEnumerable<IBackupHandler> CreateHandlers()
        {
            return serviceProvider.GetRequiredService<IEnumerable<IBackupHandler>>();
        }

        public Task<J<List<IBackupJob>>> GetStateAsync()
        {
            return J.AsTask(state.Value.Jobs.OfType<IBackupJob>().ToList());
        }
    }
}
