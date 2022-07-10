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

namespace Squidex.Domain.Apps.Entities.Backup
{
    public sealed class RestoreProcessor
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

        // Use a run to store all state that is necessary for a single run.
        private sealed class Run : IDisposable
        {
            private readonly CancellationTokenSource cancellationSource = new CancellationTokenSource();
            private readonly CancellationTokenSource cancellationLinked;
            private readonly IClock clock;

            public IEnumerable<IBackupHandler> Handlers { get; init; }

            public IBackupReader Reader { get; set; }

            public RestoreJob Job { get; init; }

            public RestoreContext Context { get; set; }

            public StreamMapper StreamMapper { get; set; }

            public CancellationToken CancellationToken => cancellationLinked.Token;

            public Run(IClock clock, CancellationToken ct)
            {
                cancellationLinked = CancellationTokenSource.CreateLinkedTokenSource(ct, cancellationSource.Token);

                this.clock = clock;
            }

            public void Log(string message, bool replace = false)
            {
                if (replace && Job.Log.Count > 0)
                {
                    Job.Log[^1] = $"{clock.GetCurrentInstant()}: {message}";
                }
                else
                {
                    Job.Log.Add($"{clock.GetCurrentInstant()}: {message}");
                }
            }

            public void Dispose()
            {
                Reader?.Dispose();

                cancellationSource.Dispose();
                cancellationLinked.Dispose();
            }

            public void Cancel()
            {
                try
                {
                    cancellationSource.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    // Cancellation token might have been disposed, if the run is completed.
                }
            }
        }

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

            state = new SimpleState<BackupRestoreState>(persistenceFactory, GetType(), "Default");
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
                var run = currentRun = new Run(Clock, ct)
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
                    await state.WriteAsync(run.CancellationToken);

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
                    run.Log("Started. The restore process has the following steps:");
                    run.Log("  * Download backup");
                    run.Log("  * Restore events and attachments.");
                    run.Log("  * Restore all objects like app, schemas and contents");
                    run.Log("  * Complete the restore operation for all objects");

                    log.LogInformation("Backup with job id {backupId} with from URL '{url}' started.", run.Job.Id, run.Job.Url);

                    run.Reader = await DownloadAsync(run, ct);

                    await run.Reader.CheckCompatibilityAsync();

                    using (Telemetry.Activities.StartActivity("ReadEvents"))
                    {
                        await ReadEventsAsync(run, ct); 
                    }

                    foreach (var handler in run.Handlers)
                    {
                        using (Telemetry.Activities.StartActivity($"{handler.GetType().Name}/RestoreAsync"))
                        {
                            await handler.RestoreAsync(run.Context, ct);
                        }

                        run.Log($"Restored {handler.Name}");
                    }

                    foreach (var handler in run.Handlers)
                    {
                        using (Telemetry.Activities.StartActivity($"{handler.GetType().Name}/CompleteRestoreAsync"))
                        {
                            await handler.CompleteRestoreAsync(run.Context, run.Job.NewAppName!);
                        }

                        run.Log($"Completed {handler.Name}");
                    }

                    await AssignContributorAsync(run);

                    run.Job.Status = JobStatus.Completed;
                    run.Log("Completed, Yeah!");

                    log.LogInformation("Backup with job id {backupId} from URL '{url}' completed.", run.Job.Id, run.Job.Url);
                }
                catch (Exception ex)
                {
                    switch (ex)
                    {
                        case BackupRestoreException backupException:
                            run.Log(backupException.Message);
                            break;
                        case FileNotFoundException fileNotFoundException:
                            run.Log(fileNotFoundException.Message);
                            break;
                        default:
                            run.Log("Failed with internal error");
                            break;
                    }

                    await CleanupAsync(run);

                    run.Job.Status = JobStatus.Failed;

                    log.LogError(ex, "Backup with job id {backupId} from URL '{url}' failed.", run.Job.Id, run.Job.Url);
                }
                finally
                {
                    run.Job.Stopped = Clock.GetCurrentInstant();

                    await state.WriteAsync(ct);
                }
            }
        }

        private async Task AssignContributorAsync(Run run)
        {
            if (run.Job.Actor?.IsUser != true)
            {
                run.Log("Current user not assigned because restore was triggered by client.");
                return;
            }

            try
            {
                var command = new AssignContributor
                {
                    Actor = run.Job.Actor,
                    AppId = run.Job.AppId,
                    ContributorId = run.Job.Actor.Identifier,
                    Restoring = true,
                    Role = Role.Owner
                };

                await commandBus.PublishAsync(command, default);

                run.Log("Assigned current user.");
            }
            catch (DomainException ex)
            {
                run.Log($"Failed to assign contributor: {ex.Message}");
            }
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
                run.Log("Downloading Backup");

                var reader = await backupArchiveLocation.OpenReaderAsync(run.Job.Url, run.Job.Id, ct);

                run.Log("Downloaded Backup");

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

                    run.Log($"Reading {run.Reader.ReadEvents}/{handled} events and {run.Reader.ReadAttachments} attachments completed.", true);
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
                    if (!await batchBlock.SendAsync((newStream, job.Event)))
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

            if (@event.Payload is SquidexEvent squidexEvent && squidexEvent.Actor != null)
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
                run.Log("Creating Users");

                await userMapping.RestoreAsync(run.Reader, userResolver, ct);

                run.Log("Created Users");
            }

            run.Context = new RestoreContext(run.Job.AppId.Id, userMapping, run.Reader, previousAppId);
        }
    }
}
