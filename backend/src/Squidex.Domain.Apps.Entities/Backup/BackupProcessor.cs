// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using NodaTime;
using Squidex.Domain.Apps.Entities.Backup.State;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Translations;
using Squidex.Shared.Users;

#pragma warning disable MA0040 // Flow the cancellation token

namespace Squidex.Domain.Apps.Entities.Backup
{
    public sealed class BackupProcessor
    {
        private static readonly Duration UpdateDuration = Duration.FromSeconds(1);
        private readonly IBackupArchiveLocation backupArchiveLocation;
        private readonly IBackupArchiveStore backupArchiveStore;
        private readonly IBackupHandlerFactory backupHandlerFactory;
        private readonly IEventFormatter eventFormatter;
        private readonly IEventStore eventStore;
        private readonly IUserResolver userResolver;
        private readonly ILogger<BackupProcessor> log;
        private readonly SimpleState<BackupState> state;
        private readonly DomainId appId;
        private CancellationTokenSource? currentJobToken;
        private BackupJob? currentJob;

        public IClock Clock { get; set; } = SystemClock.Instance;

        public BackupProcessor(
            DomainId appId,
            IBackupArchiveLocation backupArchiveLocation,
            IBackupArchiveStore backupArchiveStore,
            IBackupHandlerFactory backupHandlerFactory,
            IEventFormatter eventFormatter,
            IEventStore eventStore,
            IPersistenceFactory<BackupState> persistenceFactory,
            IUserResolver userResolver,
            ILogger<BackupProcessor> log)
        {
            this.appId = appId;
            this.backupArchiveLocation = backupArchiveLocation;
            this.backupArchiveStore = backupArchiveStore;
            this.backupHandlerFactory = backupHandlerFactory;
            this.eventFormatter = eventFormatter;
            this.eventStore = eventStore;
            this.userResolver = userResolver;
            this.log = log;

            state = new SimpleState<BackupState>(persistenceFactory, GetType(), appId);
        }

        public async Task LoadAsync(
            CancellationToken ct)
        {
            await state.LoadAsync(ct);
        }

        public async Task ClearAsync()
        {
            foreach (var backup in state.Value.Jobs)
            {
                await backupArchiveStore.DeleteAsync(backup.Id, default);
            }

            await state.ClearAsync();
        }

        public async Task BackupAsync(RefToken actor,
            CancellationToken ct)
        {
            if (currentJobToken != null)
            {
                throw new DomainException(T.Get("backups.alreadyRunning"));
            }

            state.Value.EnsureCanStart();

            var job = new BackupJob
            {
                Id = DomainId.NewGuid(),
                Started = Clock.GetCurrentInstant(),
                Status = JobStatus.Started
            };

            currentJobToken = new CancellationTokenSource();
            currentJob = job;

            state.Value.Jobs.Insert(0, job);

            await state.WriteAsync();

            using (var combined = CancellationTokenSource.CreateLinkedTokenSource(currentJobToken.Token, ct))
            {
                await ProcessAsync(job, actor, combined.Token);
            }
        }

        private async Task ProcessAsync(BackupJob job, RefToken actor,
            CancellationToken ct)
        {
            var handlers = backupHandlerFactory.CreateMany();

            var lastTimestamp = job.Started;

            try
            {
                await using (var stream = backupArchiveLocation.OpenStream(job.Id))
                {
                    using (var writer = await backupArchiveLocation.OpenWriterAsync(stream))
                    {
                        await writer.WriteVersionAsync();

                        var userMapping = new UserMapping(actor);

                        var context = new BackupContext(appId, userMapping, writer);

                        await foreach (var storedEvent in eventStore.QueryAllAsync(GetFilter(), ct: ct))
                        {
                            var @event = eventFormatter.Parse(storedEvent);

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
                log.LogError(ex, "Faield to make backup with backup id '{backupId}'.", job.Id);

                job.Status = JobStatus.Failed;
            }
            finally
            {
                job.Stopped = Clock.GetCurrentInstant();

                await state.WriteAsync();

                currentJobToken?.Dispose();
                currentJobToken = null;
                currentJob = null;
            }
        }

        private string GetFilter()
        {
            return $"^[^\\-]*-{Regex.Escape(appId.ToString())}";
        }

        private async Task<Instant> WritePeriodically(Instant lastTimestamp)
        {
            var now = Clock.GetCurrentInstant();

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
                await backupArchiveStore.DeleteAsync(job.Id);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to make remove with backup id '{backupId}'.", job.Id);
            }

            state.Value.Jobs.Remove(job);

            await state.WriteAsync();
        }
    }
}
