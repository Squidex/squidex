﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.DependencyInjection;
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
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public sealed class RestoreProcessor
    {
        private readonly IBackupArchiveLocation backupArchiveLocation;
        private readonly IBackupHandlerFactory backupHandlerFactory;
        private readonly IClock clock;
        private readonly ICommandBus commandBus;
        private readonly IEventFormatter eventFormatter;
        private readonly IEventStore eventStore;
        private readonly IEventStreamNames eventStreamNames;
        private readonly IUserResolver userResolver;
        private readonly ILogger<RestoreProcessor> log;
        private readonly SimpleState<BackupRestoreState> state;
        private RestoreContext runningContext;
        private StreamMapper runningStreamMapper;

        private RestoreJob CurrentJob
        {
            get => state.Value.Job;
        }

        public RestoreProcessor(
            IBackupArchiveLocation backupArchiveLocation,
            IBackupHandlerFactory backupHandlerFactory,
            IClock clock,
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
            this.clock = clock;
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

            await RecoverAfterRestartAsync();
        }

        private async Task RecoverAfterRestartAsync()
        {
            if (CurrentJob?.Status == JobStatus.Started)
            {
                Log("Failed due application restart");

                CurrentJob.Status = JobStatus.Failed;

                await state.WriteAsync();
            }
        }

        public async Task RestoreAsync(Uri url, RefToken actor, string? newAppName,
            CancellationToken ct)
        {
            Guard.NotNull(url);
            Guard.NotNull(actor);

            if (!string.IsNullOrWhiteSpace(newAppName))
            {
                Guard.ValidSlug(newAppName);
            }

            CurrentJob.EnsureCanStart();

            state.Value.Job = new RestoreJob
            {
                Id = DomainId.NewGuid(),
                NewAppName = newAppName,
                Actor = actor,
                Started = clock.GetCurrentInstant(),
                Status = JobStatus.Started,
                Url = url
            };

            await state.WriteAsync(ct);

            await ProcessAsync(ct);
        }

        private async Task ProcessAsync(
            CancellationToken ct)
        {
            var handlers = backupHandlerFactory.CreateMany();

            using (Telemetry.Activities.StartActivity("RestoreBackup"))
            {
                try
                {
                    Log("Started. The restore process has the following steps:");
                    Log("  * Download backup");
                    Log("  * Restore events and attachments.");
                    Log("  * Restore all objects like app, schemas and contents");
                    Log("  * Complete the restore operation for all objects");

                    log.LogInformation("Backup with job id {backupId} with from URL '{url}' started.",
                        CurrentJob.Id,
                        CurrentJob.Url);

                    using (var reader = await DownloadAsync())
                    {
                        await reader.CheckCompatibilityAsync();

                        using (Telemetry.Activities.StartActivity("ReadEvents"))
                        {
                            await ReadEventsAsync(reader, handlers);
                        }

                        foreach (var handler in handlers)
                        {
                            using (Telemetry.Activities.StartActivity($"{handler.GetType().Name}/RestoreAsync"))
                            {
                                await handler.RestoreAsync(runningContext, ct);
                            }

                            Log($"Restored {handler.Name}");
                        }

                        foreach (var handler in handlers)
                        {
                            using (Telemetry.Activities.StartActivity($"{handler.GetType().Name}/CompleteRestoreAsync"))
                            {
                                await handler.CompleteRestoreAsync(runningContext, CurrentJob.NewAppName!);
                            }

                            Log($"Completed {handler.Name}");
                        }
                    }

                    await AssignContributorAsync();

                    CurrentJob.Status = JobStatus.Completed;

                    Log("Completed, Yeah!");

                    log.LogInformation("Backup with job id {backupId} from URL '{url}' completed.",
                        CurrentJob.Id,
                        CurrentJob.Url);
                }
                catch (Exception ex)
                {
                    switch (ex)
                    {
                        case BackupRestoreException backupException:
                            Log(backupException.Message);
                            break;
                        case FileNotFoundException fileNotFoundException:
                            Log(fileNotFoundException.Message);
                            break;
                        default:
                            Log("Failed with internal error");
                            break;
                    }

                    await CleanupAsync(handlers);

                    CurrentJob.Status = JobStatus.Failed;

                    log.LogError(ex, "Backup with job id {backupId} from URL '{url}' failed.",
                        CurrentJob.Id,
                        CurrentJob.Url);
                }
                finally
                {
                    CurrentJob.Stopped = clock.GetCurrentInstant();

                    await state.WriteAsync(ct);

                    runningStreamMapper = null!;
                    runningContext = null!;
                }
            }
        }

        private async Task AssignContributorAsync()
        {
            var actor = CurrentJob.Actor;

            if (actor?.IsUser == true)
            {
                try
                {
                    await commandBus.PublishAsync(new AssignContributor
                    {
                        Actor = actor,
                        AppId = CurrentJob.AppId,
                        ContributorId = actor.Identifier,
                        Restoring = true,
                        Role = Role.Owner
                    });

                    Log("Assigned current user.");
                }
                catch (DomainException ex)
                {
                    Log($"Failed to assign contributor: {ex.Message}");
                }
            }
            else
            {
                Log("Current user not assigned because restore was triggered by client.");
            }
        }

        private async Task CleanupAsync(IEnumerable<IBackupHandler> handlers)
        {
            if (CurrentJob.AppId != null)
            {
                var appId = CurrentJob.AppId.Id;

                foreach (var handler in handlers)
                {
                    try
                    {
                        await handler.CleanupRestoreErrorAsync(appId);
                    }
                    catch (Exception ex)
                    {
                        log.LogError(ex, "Failed to clean up restore.");
                    }
                }
            }
        }

        private async Task<IBackupReader> DownloadAsync()
        {
            using (Telemetry.Activities.StartActivity("Download"))
            {
                Log("Downloading Backup");

                var reader = await backupArchiveLocation.OpenReaderAsync(CurrentJob.Url, CurrentJob.Id);

                Log("Downloaded Backup");

                return reader;
            }
        }

        private async Task ReadEventsAsync(IBackupReader reader, IEnumerable<IBackupHandler> handlers)
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
                        var offset = runningStreamMapper.GetStreamOffset(stream);

                        commits.Add(EventCommit.Create(stream, offset, @event, eventFormatter));
                    }

                    await eventStore.AppendUnsafeAsync(commits);

                    handled += commits.Count;

                    Log($"Reading {reader.ReadEvents}/{handled} events and {reader.ReadAttachments} attachments completed.", true);
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

            await foreach (var job in reader.ReadEventsAsync(eventStreamNames, eventFormatter))
            {
                var newStream = await HandleEventAsync(reader, handlers, job.Stream, job.Event);

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

        private async Task<string?> HandleEventAsync(IBackupReader reader, IEnumerable<IBackupHandler> handlers, string stream, Envelope<IEvent> @event,
            CancellationToken ct = default)
        {
            if (@event.Payload is AppCreated appCreated)
            {
                var previousAppId = appCreated.AppId.Id;

                if (!string.IsNullOrWhiteSpace(CurrentJob.NewAppName))
                {
                    appCreated.Name = CurrentJob.NewAppName;

                    CurrentJob.AppId = NamedId.Of(DomainId.NewGuid(), CurrentJob.NewAppName);
                }
                else
                {
                    CurrentJob.AppId = NamedId.Of(DomainId.NewGuid(), appCreated.Name);
                }

                await CreateContextAsync(reader, previousAppId);
            }

            if (@event.Payload is SquidexEvent squidexEvent && squidexEvent.Actor != null)
            {
                if (runningContext.UserMapping.TryMap(squidexEvent.Actor, out var newUser))
                {
                    squidexEvent.Actor = newUser;
                }
            }

            if (@event.Payload is AppEvent appEvent)
            {
                appEvent.AppId = CurrentJob.AppId;
            }

            var (newStream, id) = runningStreamMapper.Map(stream);

            @event.SetAggregateId(id);
            @event.SetRestored();

            foreach (var handler in handlers)
            {
                if (!await handler.RestoreEventAsync(@event, runningContext, ct))
                {
                    return null;
                }
            }

            return newStream;
        }

        private async Task CreateContextAsync(IBackupReader reader, DomainId previousAppId)
        {
            var userMapping = new UserMapping(CurrentJob.Actor);

            using (Telemetry.Activities.StartActivity("CreateUsers"))
            {
                Log("Creating Users");

                await userMapping.RestoreAsync(reader, userResolver);

                Log("Created Users");
            }

            runningContext = new RestoreContext(CurrentJob.AppId.Id, userMapping, reader, previousAppId);
            runningStreamMapper = new StreamMapper(runningContext);
        }

        private void Log(string message, bool replace = false)
        {
            if (replace && CurrentJob.Log.Count > 0)
            {
                CurrentJob.Log[^1] = $"{clock.GetCurrentInstant()}: {message}";
            }
            else
            {
                CurrentJob.Log.Add($"{clock.GetCurrentInstant()}: {message}");
            }
        }
    }
}
