﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Backup.State;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public sealed class RestoreGrain : GrainOfString, IRestoreGrain
    {
        private readonly IBackupArchiveLocation backupArchiveLocation;
        private readonly IClock clock;
        private readonly ICommandBus commandBus;
        private readonly IEventStore eventStore;
        private readonly IEventDataFormatter eventDataFormatter;
        private readonly ISemanticLog log;
        private readonly IServiceProvider serviceProvider;
        private readonly IStreamNameResolver streamNameResolver;
        private readonly IUserResolver userResolver;
        private readonly IGrainState<RestoreState2> state;
        private RestoreContext restoreContext;

        private RestoreJob CurrentJob
        {
            get { return state.Value.Job; }
        }

        public RestoreGrain(
            IBackupArchiveLocation backupArchiveLocation,
            IClock clock,
            ICommandBus commandBus,
            IEventDataFormatter eventDataFormatter,
            IEventStore eventStore,
            IGrainState<RestoreState2> state,
            IServiceProvider serviceProvider,
            IStreamNameResolver streamNameResolver,
            IUserResolver userResolver,
            ISemanticLog log)
        {
            Guard.NotNull(backupArchiveLocation);
            Guard.NotNull(clock);
            Guard.NotNull(commandBus);
            Guard.NotNull(eventDataFormatter);
            Guard.NotNull(eventStore);
            Guard.NotNull(serviceProvider);
            Guard.NotNull(state);
            Guard.NotNull(streamNameResolver);
            Guard.NotNull(userResolver);
            Guard.NotNull(log);

            this.backupArchiveLocation = backupArchiveLocation;
            this.clock = clock;
            this.commandBus = commandBus;
            this.eventDataFormatter = eventDataFormatter;
            this.eventStore = eventStore;
            this.serviceProvider = serviceProvider;
            this.state = state;
            this.streamNameResolver = streamNameResolver;
            this.userResolver = userResolver;
            this.log = log;
        }

        protected override Task OnActivateAsync(string key)
        {
            RecoverAfterRestartAsync().Forget();

            return TaskHelper.Done;
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

        public async Task RestoreAsync(Uri url, RefToken actor, string? newAppName)
        {
            Guard.NotNull(url);
            Guard.NotNull(actor);

            if (!string.IsNullOrWhiteSpace(newAppName))
            {
                Guard.ValidSlug(newAppName);
            }

            if (CurrentJob?.Status == JobStatus.Started)
            {
                throw new DomainException("A restore operation is already running.");
            }

            state.Value.Job = new RestoreJob
            {
                Id = Guid.NewGuid(),
                NewAppName = newAppName,
                Actor = actor,
                Started = clock.GetCurrentInstant(),
                Status = JobStatus.Started,
                Url = url
            };

            await state.WriteAsync();

            Process();
        }

        private void Process()
        {
            ProcessAsync().Forget();
        }

        private async Task ProcessAsync()
        {
            var handlers = CreateHandlers();

            var logContext = (jobId: CurrentJob.Id.ToString(), jobUrl: CurrentJob.Url.ToString());

            using (Profiler.StartSession())
            {
                try
                {
                    Log("Started. The restore process has the following steps:");
                    Log("  * Download backup");
                    Log("  * Restore events and attachments.");
                    Log("  * Restore all objects like app, schemas and contents");
                    Log("  * Complete the restore operation for all objects");

                    log.LogInformation(logContext, (ctx, w) => w
                        .WriteProperty("action", "restore")
                        .WriteProperty("status", "started")
                        .WriteProperty("operationId", ctx.jobId)
                        .WriteProperty("url", ctx.jobUrl));

                    using (var reader = await DownloadAsync())
                    {
                        using (Profiler.Trace("ReadEvents"))
                        {
                            await ReadEventsAsync(reader, handlers);
                        }

                        foreach (var handler in handlers)
                        {
                            using (Profiler.TraceMethod(handler.GetType(), nameof(IBackupHandler.RestoreAsync)))
                            {
                                await handler.RestoreAsync(restoreContext);
                            }

                            Log($"Restored {handler.Name}");
                        }

                        foreach (var handler in handlers)
                        {
                            using (Profiler.TraceMethod(handler.GetType(), nameof(IBackupHandler.CompleteRestoreAsync)))
                            {
                                await handler.CompleteRestoreAsync(restoreContext);
                            }

                            Log($"Completed {handler.Name}");
                        }
                    }

                    await AssignContributorAsync();

                    CurrentJob.Status = JobStatus.Completed;

                    Log("Completed, Yeah!");

                    log.LogInformation(logContext, (ctx, w) =>
                    {
                        w.WriteProperty("action", "restore");
                        w.WriteProperty("status", "completed");
                        w.WriteProperty("operationId", ctx.jobId);
                        w.WriteProperty("url", ctx.jobUrl);

                        Profiler.Session?.Write(w);
                    });
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

                    log.LogError(ex, logContext, (ctx, w) =>
                    {
                        w.WriteProperty("action", "retore");
                        w.WriteProperty("status", "failed");
                        w.WriteProperty("operationId", ctx.jobId);
                        w.WriteProperty("url", ctx.jobUrl);

                        Profiler.Session?.Write(w);
                    });
                }
                finally
                {
                    CurrentJob.Stopped = clock.GetCurrentInstant();

                    await state.WriteAsync();
                }
            }
        }

        private async Task AssignContributorAsync()
        {
            var actor = CurrentJob.Actor;

            if (actor?.IsSubject == true)
            {
                try
                {
                    await commandBus.PublishAsync(new AssignContributor
                    {
                        Actor = actor,
                        AppId = CurrentJob.AppId.Id,
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
                        log.LogError(ex, appId.ToString(), (logOperationId, w) => w
                            .WriteProperty("action", "cleanupRestore")
                            .WriteProperty("status", "failed")
                            .WriteProperty("operationId", logOperationId));
                    }
                }
            }
        }

        private async Task<IBackupReader> DownloadAsync()
        {
            using (Profiler.Trace("Download"))
            {
                Log("Downloading Backup");

                var reader = await backupArchiveLocation.OpenReaderAsync(CurrentJob.Url, CurrentJob.Id);

                Log("Downloaded Backup");

                return reader;
            }
        }

        private async Task ReadEventsAsync(IBackupReader reader, IEnumerable<IBackupHandler> handlers)
        {
            await reader.ReadEventsAsync(streamNameResolver, eventDataFormatter, async storedEvent =>
            {
                await HandleEventAsync(reader, handlers, storedEvent.Stream, storedEvent.Event);
            });

            Log($"Reading {reader.ReadEvents} events and {reader.ReadAttachments} attachments completed.", true);
        }

        private async Task HandleEventAsync(IBackupReader reader, IEnumerable<IBackupHandler> handlers, string stream, Envelope<IEvent> @event)
        {
            if (@event.Payload is AppCreated appCreated)
            {
                if (!string.IsNullOrWhiteSpace(CurrentJob.NewAppName))
                {
                    appCreated.Name = CurrentJob.NewAppName;

                    CurrentJob.AppId = NamedId.Of(appCreated.AppId.Id, CurrentJob.NewAppName);
                }
                else
                {
                    CurrentJob.AppId = appCreated.AppId;
                }

                await CreateContextAsync(reader);
            }

            if (@event.Payload is SquidexEvent squidexEvent)
            {
                if (restoreContext.UserMapping.TryMap(squidexEvent.Actor, out var newUser))
                {
                    squidexEvent.Actor = newUser;
                }
            }

            if (@event.Payload is AppEvent appEvent)
            {
                appEvent.AppId = CurrentJob.AppId;
            }

            foreach (var handler in handlers)
            {
                if (!await handler.RestoreEventAsync(@event, restoreContext))
                {
                    return;
                }
            }

            var eventData = eventDataFormatter.ToEventData(@event, @event.Headers.CommitId());
            var eventCommit = new List<EventData> { eventData };

            await eventStore.AppendAsync(Guid.NewGuid(), stream, eventCommit);

            Log($"Read {reader.ReadEvents} events and {reader.ReadAttachments} attachments.", true);
        }

        private async Task CreateContextAsync(IBackupReader reader)
        {
            var userMapping = new UserMapping(CurrentJob.Actor);

            using (Profiler.Trace("CreateUsers"))
            {
                Log("Creating Users");

                await userMapping.RestoreAsync(reader, userResolver);

                Log("Created Users");
            }

            restoreContext = new RestoreContext(CurrentJob.AppId.Id, userMapping, reader);
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

        private IEnumerable<IBackupHandler> CreateHandlers()
        {
            return serviceProvider.GetRequiredService<IEnumerable<IBackupHandler>>();
        }

        public Task<J<IRestoreJob>> GetStateAsync()
        {
            return Task.FromResult<J<IRestoreJob>>(CurrentJob);
        }
    }
}
