// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Orleans.Concurrency;
using Squidex.Domain.Apps.Entities.Backup.State;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.Translations;
using Squidex.Log;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Backup
{
    [Reentrant]
    public sealed class BackupGrain : GrainOfString, IBackupGrain
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
        private CancellationTokenSource? currentJobToken;
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

        protected override Task OnActivateAsync(string key)
        {
            RecoverAfterRestartAsync().Forget();

            return Task.CompletedTask;
        }

        private async Task RecoverAfterRestartAsync()
        {
            state.Value.Jobs.RemoveAll(x => x.Stopped == null);

            await state.WriteAsync();
        }

        public async Task ClearAsync()
        {
            foreach (var backup in state.Value.Jobs)
            {
#pragma warning disable MA0040 // Flow the cancellation token
                await backupArchiveStore.DeleteAsync(backup.Id);
#pragma warning restore MA0040 // Flow the cancellation token
            }

            TryDeactivateOnIdle();

            await state.ClearAsync();
        }

        public async Task BackupAsync(RefToken actor)
        {
            if (currentJobToken != null)
            {
                throw new DomainException(T.Get("backups.alreadyRunning"));
            }

            if (state.Value.Jobs.Count >= MaxBackups)
            {
                throw new DomainException(T.Get("backups.maxReached", new { max = MaxBackups }));
            }

            var job = new BackupJob
            {
                Id = DomainId.NewGuid(),
                Started = clock.GetCurrentInstant(),
                Status = JobStatus.Started
            };

            currentJobToken = new CancellationTokenSource();
            currentJob = job;

            state.Value.Jobs.Insert(0, job);

            await state.WriteAsync();

#pragma warning disable MA0042 // Do not use blocking calls in an async method
            Process(job, actor, currentJobToken.Token);
#pragma warning restore MA0042 // Do not use blocking calls in an async method
        }

        private void Process(BackupJob job, RefToken actor,
            CancellationToken ct)
        {
            ProcessAsync(job, actor, ct).Forget();
        }

        private async Task ProcessAsync(BackupJob job, RefToken actor,
            CancellationToken ct)
        {
            var handlers = CreateHandlers();

            var lastTimestamp = job.Started;

            try
            {
                var appId = DomainId.Create(Key);

                await using (var stream = backupArchiveLocation.OpenStream(job.Id))
                {
                    using (var writer = await backupArchiveLocation.OpenWriterAsync(stream))
                    {
                        await writer.WriteVersionAsync();

                        var userMapping = new UserMapping(actor);

                        var context = new BackupContext(appId, userMapping, writer);

                        await foreach (var storedEvent in eventStore.QueryAllAsync(GetFilter(), ct: ct))
                        {
                            var @event = eventDataFormatter.Parse(storedEvent);

                            if (@event.Payload is SquidexEvent squidexEvent && squidexEvent.Actor != null)
                            {
                                context.UserMapping.Backup(squidexEvent.Actor);
                            }

                            foreach (var handler in handlers)
                            {
                                await handler.BackupEventAsync(@event, context, ct);
                            }

                            writer.WriteEvent(storedEvent, ct);

                            job.HandledEvents = writer.WrittenEvents;
                            job.HandledAssets = writer.WrittenAttachments;

                            lastTimestamp = await WritePeriodically(lastTimestamp);
                        }

                        foreach (var handler in handlers)
                        {
                            ct.ThrowIfCancellationRequested();

                            await handler.BackupAsync(context, ct);
                        }

                        foreach (var handler in handlers)
                        {
                            ct.ThrowIfCancellationRequested();

                            await handler.CompleteBackupAsync(context);
                        }

                        await userMapping.StoreAsync(writer, userResolver, ct);
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

                currentJobToken?.Dispose();
                currentJobToken = null;
                currentJob = null;
            }
        }

        private string GetFilter()
        {
            return $"^[^\\-]*-{Regex.Escape(Key)}";
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

        public async Task DeleteAsync(DomainId id)
        {
            var job = state.Value.Jobs.Find(x => x.Id == id);

            if (job == null)
            {
                throw new DomainObjectNotFoundException(id.ToString());
            }

            if (currentJob == job)
            {
                try
                {
                    currentJobToken?.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
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
#pragma warning disable MA0040 // Flow the cancellation token
                await backupArchiveStore.DeleteAsync(job.Id);
#pragma warning restore MA0040 // Flow the cancellation token
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
