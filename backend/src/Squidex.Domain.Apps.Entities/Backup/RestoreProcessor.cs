// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using NodaTime;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Backup.State;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.Translations;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Backup;

public sealed partial class RestoreProcessor
{
    private readonly IBackupArchiveLocation backupArchiveLocation;
    private readonly IBackupHandlerFactory backupHandlerFactory;
    private readonly ICommandBus commandBus;
    private readonly IEventFormatter eventFormatter;
    private readonly IEventStore eventStore;
    private readonly IEventStreamNames eventStreamNames;
    private readonly IUserResolver userResolver;
    private readonly ILogger<RestoreProcessor> log;
    private readonly ReentrantScheduler scheduler = new ReentrantScheduler(1);
    private readonly SimpleState<BackupRestoreState> state;
    private Run? currentRun;

    public IClock Clock { get; set; } = SystemClock.Instance;

    public RestoreProcessor(
        IBackupArchiveLocation backupArchiveLocation,
        IBackupHandlerFactory backupHandlerFactory,
        ICommandBus commandBus,
        IEventFormatter eventFormatter,
        IEventStore eventStore,
        IEventStreamNames eventStreamNames,
        IPersistenceFactory<BackupRestoreState> persistenceFactory,
        IUserResolver userResolver,
        ILogger<RestoreProcessor> log)
    {
        this.backupArchiveLocation = backupArchiveLocation;
        this.backupHandlerFactory = backupHandlerFactory;
        this.commandBus = commandBus;
        this.eventFormatter = eventFormatter;
        this.eventStore = eventStore;
        this.eventStreamNames = eventStreamNames;
        this.userResolver = userResolver;
        this.log = log;

        // Enable locking for the parallel operations that might write stuff.
        state = new SimpleState<BackupRestoreState>(persistenceFactory, GetType(), "Default", true);
    }

    public async Task LoadAsync(
        CancellationToken ct)
    {
        await state.LoadAsync(ct);

        if (state.Value.Job?.Status == JobStatus.Started)
        {
            state.Value.Job.Status = JobStatus.Failed;

            await state.WriteAsync(ct);
        }
    }

    public Task RestoreAsync(Uri url, RefToken actor, string? newAppName,
        CancellationToken ct)
    {
        Guard.NotNull(url);
        Guard.NotNull(actor);

        if (!string.IsNullOrWhiteSpace(newAppName))
        {
            Guard.ValidSlug(newAppName);
        }

        return scheduler.ScheduleAsync(async ct =>
        {
            if (currentRun != null)
            {
                throw new DomainException(T.Get("backups.restoreRunning"));
            }

            state.Value.Job?.EnsureCanStart();

            // Set the current run first to indicate that we are running a rule at the moment.
            var run = currentRun = new Run(ct)
            {
                Job = new RestoreJob
                {
                    Id = DomainId.NewGuid(),
                    NewAppName = newAppName,
                    Actor = actor,
                    Started = Clock.GetCurrentInstant(),
                    Status = JobStatus.Started,
                    Url = url
                },
                Handlers = backupHandlerFactory.CreateMany()
            };

            state.Value.Job = run.Job;
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
        using (Telemetry.Activities.StartActivity("RestoreBackup"))
        {
            try
            {
                await state.WriteAsync(run.CancellationToken);

                await LogAsync(run, "Started. The restore process has the following steps:");
                await LogAsync(run, "  * Download backup");
                await LogAsync(run, "  * Restore events and attachments.");
                await LogAsync(run, "  * Restore all objects like app, schemas and contents");
                await LogAsync(run, "  * Complete the restore operation for all objects");

                log.LogInformation("Backup with job id {backupId} with from URL '{url}' started.", run.Job.Id, run.Job.Url);

                run.Reader = await DownloadAsync(run, ct);

                await run.Reader.CheckCompatibilityAsync();

                using (Telemetry.Activities.StartActivity("ReadEvents"))
                {
                    await ReadEventsAsync(run, ct);
                }

                if (run.Context == null)
                {
                    throw new BackupRestoreException("Backup has no event.");
                }

                foreach (var handler in run.Handlers)
                {
                    using (Telemetry.Activities.StartActivity($"{handler.GetType().Name}/RestoreAsync"))
                    {
                        await handler.RestoreAsync(run.Context, ct);
                    }

                    await LogAsync(run, $"Restored {handler.Name}");
                }

                foreach (var handler in run.Handlers)
                {
                    using (Telemetry.Activities.StartActivity($"{handler.GetType().Name}/CompleteRestoreAsync"))
                    {
                        await handler.CompleteRestoreAsync(run.Context, run.Job.NewAppName!);
                    }

                    await LogAsync(run, $"Completed {handler.Name}");
                }

                // Add the current user to the app, so that the admin can see it and verify integrity.
                await AssignContributorAsync(run);

                await SetStatusAsync(run, JobStatus.Completed, "Completed, Yeah!");

                log.LogInformation("Backup with job id {backupId} from URL '{url}' completed.", run.Job.Id, run.Job.Url);
            }
            catch (Exception ex)
            {
                // Cleanup as soon as possible.
                await CleanupAsync(run);

                var message = "Failed with internal error.";

                switch (ex)
                {
                    case BackupRestoreException backupException:
                        message = backupException.Message;
                        break;
                    case FileNotFoundException fileNotFoundException:
                        message = fileNotFoundException.Message;
                        break;
                }

                await SetStatusAsync(run, JobStatus.Failed, message);

                log.LogError(ex, "Backup with job id {backupId} from URL '{url}' failed.", run.Job.Id, run.Job.Url);
            }
        }
    }

    private async Task AssignContributorAsync(Run run)
    {
        if (run.Job.Actor?.IsUser != true)
        {
            await LogAsync(run, "Current user not assigned because restore was triggered by client.");
            return;
        }

        try
        {
            // Add the current user to the app, so that the admin can see it and verify integrity.
            await PublishAsync(run, new AssignContributor
            {
                ContributorId = run.Job.Actor.Identifier,
                IgnoreActor = true,
                IgnorePlans = true,
                Role = Role.Owner
            });

            await LogAsync(run, "Assigned current user.");
        }
        catch (DomainException ex)
        {
            await LogAsync(run, $"Failed to assign contributor: {ex.Message}");
        }
    }

    private Task PublishAsync(Run run, AppCommand command)
    {
        command.Actor = run.Job.Actor;

        if (command is IAppCommand appCommand)
        {
            appCommand.AppId = run.Job.AppId;
        }

        return commandBus.PublishAsync(command, default);
    }

    private async Task CleanupAsync(Run run)
    {
        if (run.Job.AppId == null)
        {
            return;
        }

        foreach (var handler in run.Handlers)
        {
            try
            {
                await handler.CleanupRestoreErrorAsync(run.Job.AppId.Id);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to clean up restore.");
            }
        }
    }

    private async Task<IBackupReader> DownloadAsync(Run run,
        CancellationToken ct)
    {
        using (Telemetry.Activities.StartActivity("Download"))
        {
            await LogAsync(run, "Downloading Backup");

            var reader = await backupArchiveLocation.OpenReaderAsync(run.Job.Url, run.Job.Id, ct);

            await LogAsync(run, "Downloaded Backup");

            return reader;
        }
    }

    private async Task ReadEventsAsync(Run run,
        CancellationToken ct)
    {
        const int BatchSize = 100;

        var handled = 0;

        var writeBlock = new ActionBlock<(string, Envelope<IEvent>)[]>(async batch =>
        {
            try
            {
                var commits = new List<EventCommit>(batch.Length);

                foreach (var (stream, @event) in batch)
                {
                    var offset = run.StreamMapper.GetStreamOffset(stream);

                    commits.Add(EventCommit.Create(stream, offset, @event, eventFormatter));
                }

                await eventStore.AppendUnsafeAsync(commits, ct);

                handled += commits.Count;

                await LogAsync(run, $"Reading {run.Reader.ReadEvents}/{handled} events and {run.Reader.ReadAttachments} attachments completed.", true);
            }
            catch (OperationCanceledException ex)
            {
                // Dataflow swallows operation cancelled exception.
                throw new AggregateException(ex);
            }
        }, new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = 1,
            MaxMessagesPerTask = 1,
            BoundedCapacity = 2
        });

        var batchBlock = new BatchBlock<(string, Envelope<IEvent>)>(BatchSize, new GroupingDataflowBlockOptions
        {
            BoundedCapacity = BatchSize * 2
        });

        batchBlock.BidirectionalLinkTo(writeBlock);

        await foreach (var job in run.Reader.ReadEventsAsync(eventStreamNames, eventFormatter, ct))
        {
            var newStream = await HandleEventAsync(run, job.Stream, job.Event, ct);

            if (newStream != null)
            {
                if (!await batchBlock.SendAsync((newStream, job.Event), ct))
                {
                    break;
                }
            }
        }

        batchBlock.Complete();

        await writeBlock.Completion;
    }

    private async Task<string?> HandleEventAsync(Run run, string stream, Envelope<IEvent> @event,
        CancellationToken ct = default)
    {
        if (@event.Payload is AppCreated appCreated)
        {
            var previousAppId = appCreated.AppId.Id;

            if (!string.IsNullOrWhiteSpace(run.Job.NewAppName))
            {
                appCreated.Name = run.Job.NewAppName;

                run.Job.AppId = NamedId.Of(DomainId.NewGuid(), run.Job.NewAppName);
            }
            else
            {
                run.Job.AppId = NamedId.Of(DomainId.NewGuid(), appCreated.Name);
            }

            await CreateContextAsync(run, previousAppId, ct);

            run.StreamMapper = new StreamMapper(run.Context);
        }

        if (@event.Payload is SquidexEvent { Actor: { } } squidexEvent)
        {
            if (run.Context.UserMapping.TryMap(squidexEvent.Actor, out var newUser))
            {
                squidexEvent.Actor = newUser;
            }
        }

        if (@event.Payload is AppEvent appEvent)
        {
            appEvent.AppId = run.Job.AppId;
        }

        var (newStream, id) = run.StreamMapper.Map(stream);

        @event.SetAggregateId(id);
        @event.SetRestored();

        foreach (var handler in run.Handlers)
        {
            if (!await handler.RestoreEventAsync(@event, run.Context, ct))
            {
                return null;
            }
        }

        return newStream;
    }

    private async Task CreateContextAsync(Run run, DomainId previousAppId,
        CancellationToken ct)
    {
        var userMapping = new UserMapping(run.Job.Actor);

        using (Telemetry.Activities.StartActivity("CreateUsers"))
        {
            await LogAsync(run, "Creating Users");

            await userMapping.RestoreAsync(run.Reader, userResolver, ct);

            await LogAsync(run, "Created Users");
        }

        run.Context = new RestoreContext(run.Job.AppId.Id, userMapping, run.Reader, previousAppId);
    }

    private Task SetStatusAsync(Run run, JobStatus status, string message)
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

        run.Job.Log.Add($"{now}: {message}");

        return state.WriteAsync(ct: default);
    }

    private Task LogAsync(Run run, string message, bool replace = false)
    {
        var now = Clock.GetCurrentInstant();

        if (replace && run.Job.Log.Count > 0)
        {
            run.Job.Log[^1] = $"{now}: {message}";
        }
        else
        {
            run.Job.Log.Add($"{now}: {message}");
        }

        return state.WriteAsync(100, run.CancellationToken);
    }
}
