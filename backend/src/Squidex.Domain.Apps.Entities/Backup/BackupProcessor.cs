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
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.Translations;
using Squidex.Shared.Users;

#pragma warning disable MA0040 // Flow the cancellation token

namespace Squidex.Domain.Apps.Entities.Backup;

public sealed partial class BackupProcessor
{
    private readonly IBackupArchiveLocation backupArchiveLocation;
    private readonly IBackupArchiveStore backupArchiveStore;
    private readonly IBackupHandlerFactory backupHandlerFactory;
    private readonly IEventFormatter eventFormatter;
    private readonly IEventStore eventStore;
    private readonly IUserResolver userResolver;
    private readonly ILogger<BackupProcessor> log;
    private readonly SimpleState<BackupState> state;
    private readonly ReentrantScheduler scheduler = new ReentrantScheduler(1);
    private readonly DomainId appId;
    private Run? currentRun;

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

        // Enable locking for the parallel operations that might write stuff.
        state = new SimpleState<BackupState>(persistenceFactory, GetType(), appId, true);
    }

    public async Task LoadAsync(
        CancellationToken ct)
    {
        await state.LoadAsync(ct);

        if (state.Value.Jobs.RemoveAll(x => x.Stopped == null) > 0)
        {
            // This should actually never happen, so we log with warning.
            log.LogWarning("Removed unfinished backups for app {appId} after start.", appId);

            await state.WriteAsync(ct);
        }
    }

    public Task ClearAsync()
    {
        return scheduler.ScheduleAsync(async _ =>
        {
            log.LogInformation("Clearing backups for app {appId}.", appId);

            foreach (var backup in state.Value.Jobs)
            {
                await backupArchiveStore.DeleteAsync(backup.Id, default);
            }

            await state.ClearAsync(default);
        });
    }

    public Task BackupAsync(RefToken actor,
        CancellationToken ct)
    {
        return scheduler.ScheduleAsync(async _ =>
        {
            if (currentRun != null)
            {
                throw new DomainException(T.Get("backups.alreadyRunning"));
            }

            state.Value.EnsureCanStart();

            // Set the current run first to indicate that we are running a rule at the moment.
            var run = currentRun = new Run(ct)
            {
                Actor = actor,
                Job = new BackupJob
                {
                    Id = DomainId.NewGuid(),
                    Started = Clock.GetCurrentInstant(),
                    Status = JobStatus.Started
                },
                Handlers = backupHandlerFactory.CreateMany()
            };

            log.LogInformation("Starting new backup with backup id '{backupId}' for app {appId}.", run.Job.Id, appId);

            state.Value.Jobs.Insert(0, run.Job);
            try
            {
                await ProcessAsync(run, run.CancellationToken);
            }
            finally
            {
                // Unset the run to indicate that we are done.
                currentRun.Dispose();
                currentRun = null;
            }
        }, ct);
    }

    private async Task ProcessAsync(Run run,
        CancellationToken ct)
    {
        try
        {
            await state.WriteAsync(run.CancellationToken);

            await using (var stream = backupArchiveLocation.OpenStream(run.Job.Id))
            {
                using (var writer = await backupArchiveLocation.OpenWriterAsync(stream, ct))
                {
                    await writer.WriteVersionAsync();

                    var backupUsers = new UserMapping(run.Actor);
                    var backupContext = new BackupContext(appId, backupUsers, writer);

                    await foreach (var storedEvent in eventStore.QueryAllAsync(GetFilter(), ct: ct))
                    {
                        var @event = eventFormatter.Parse(storedEvent);

                        if (@event.Payload is SquidexEvent { Actor: { } } squidexEvent)
                        {
                            backupUsers.Backup(squidexEvent.Actor);
                        }

                        foreach (var handler in run.Handlers)
                        {
                            await handler.BackupEventAsync(@event, backupContext, ct);
                        }

                        writer.WriteEvent(storedEvent, ct);

                        await LogAsync(run, writer.WrittenEvents, writer.WrittenAttachments);
                    }

                    foreach (var handler in run.Handlers)
                    {
                        ct.ThrowIfCancellationRequested();

                        await handler.BackupAsync(backupContext, ct);
                    }

                    foreach (var handler in run.Handlers)
                    {
                        ct.ThrowIfCancellationRequested();

                        await handler.CompleteBackupAsync(backupContext);
                    }

                    await backupUsers.StoreAsync(writer, userResolver, ct);
                }

                stream.Position = 0;

                ct.ThrowIfCancellationRequested();

                await backupArchiveStore.UploadAsync(run.Job.Id, stream, ct);
            }

            await SetStatusAsync(run, JobStatus.Completed);
        }
        catch (Exception ex)
        {
            await SetStatusAsync(run, JobStatus.Failed);

            log.LogError(ex, "Failed to make backup with backup id '{backupId}'.", run.Job.Id);
        }
    }

    private string GetFilter()
    {
        return $"^[^\\-]*-{Regex.Escape(appId.ToString())}";
    }

    public Task DeleteAsync(DomainId id)
    {
        return scheduler.ScheduleAsync(async _ =>
        {
            var job = state.Value.Jobs.Find(x => x.Id == id);

            if (job == null)
            {
                throw new DomainObjectNotFoundException(id.ToString());
            }

            log.LogInformation("Deleting backup with backup id '{backupId}' for app {appId}.", job.Id, appId);

            if (currentRun?.Job == job)
            {
                currentRun.Cancel();
            }
            else
            {
                await RemoveAsync(job);
            }
        });
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

    private Task SetStatusAsync(Run run, JobStatus status)
    {
        var now = Clock.GetCurrentInstant();

        run.Job.Status = status;

        if (status == JobStatus.Failed || status == JobStatus.Completed)
        {
            run.Job.Stopped = now;
        }
        else if (status == JobStatus.Started)
        {
            run.Job.Started = now;
        }

        return state.WriteAsync(ct: default);
    }

    private Task LogAsync(Run run, int numEvents, int numAttachments)
    {
        run.Job.HandledEvents = numEvents;
        run.Job.HandledAssets = numAttachments;

        return state.WriteAsync(100, run.CancellationToken);
    }
}
