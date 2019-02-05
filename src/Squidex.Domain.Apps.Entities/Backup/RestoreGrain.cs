// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NodaTime;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Backup.Helpers;
using Squidex.Domain.Apps.Entities.Backup.State;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public sealed class RestoreGrain : GrainOfString<RestoreState>, IRestoreGrain
    {
        private readonly IBackupArchiveLocation backupArchiveLocation;
        private readonly IClock clock;
        private readonly ICommandBus commandBus;
        private readonly IEnumerable<BackupHandler> handlers;
        private readonly IJsonSerializer serializer;
        private readonly IEventStore eventStore;
        private readonly IEventDataFormatter eventDataFormatter;
        private readonly ISemanticLog log;
        private readonly IStreamNameResolver streamNameResolver;

        private RestoreStateJob CurrentJob
        {
            get { return State.Job; }
        }

        public RestoreGrain(IBackupArchiveLocation backupArchiveLocation,
            IClock clock,
            ICommandBus commandBus,
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            IEnumerable<BackupHandler> handlers,
            IJsonSerializer serializer,
            ISemanticLog log,
            IStreamNameResolver streamNameResolver,
            IStore<string> store)
            : base(store)
        {
            Guard.NotNull(backupArchiveLocation, nameof(backupArchiveLocation));
            Guard.NotNull(clock, nameof(clock));
            Guard.NotNull(commandBus, nameof(commandBus));
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(eventDataFormatter, nameof(eventDataFormatter));
            Guard.NotNull(handlers, nameof(handlers));
            Guard.NotNull(serializer, nameof(serializer));
            Guard.NotNull(store, nameof(store));
            Guard.NotNull(streamNameResolver, nameof(streamNameResolver));
            Guard.NotNull(log, nameof(log));

            this.backupArchiveLocation = backupArchiveLocation;
            this.clock = clock;
            this.commandBus = commandBus;
            this.eventStore = eventStore;
            this.eventDataFormatter = eventDataFormatter;
            this.handlers = handlers;
            this.serializer = serializer;
            this.streamNameResolver = streamNameResolver;
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

                await CleanupAsync();
                await WriteStateAsync();
            }
        }

        public Task RestoreAsync(Uri url, RefToken actor, string newAppName)
        {
            Guard.NotNull(url, nameof(url));
            Guard.NotNull(actor, nameof(actor));

            if (!string.IsNullOrWhiteSpace(newAppName))
            {
                Guard.ValidSlug(newAppName, nameof(newAppName));
            }

            if (CurrentJob?.Status == JobStatus.Started)
            {
                throw new DomainException("A restore operation is already running.");
            }

            State.Job = new RestoreStateJob
            {
                Id = Guid.NewGuid(),
                NewAppName = newAppName,
                Actor = actor,
                Started = clock.GetCurrentInstant(),
                Status = JobStatus.Started,
                Url = url
            };

            Process();

            return TaskHelper.Done;
        }

        private void Process()
        {
            ProcessAsync().Forget();
        }

        private async Task ProcessAsync()
        {
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

                    using (Profiler.Trace("Download"))
                    {
                        await DownloadAsync();
                    }

                    using (var reader = await backupArchiveLocation.OpenArchiveAsync(CurrentJob.Id, serializer))
                    {
                        using (Profiler.Trace("ReadEvents"))
                        {
                            await ReadEventsAsync(reader);
                        }

                        foreach (var handler in handlers)
                        {
                            using (Profiler.TraceMethod(handler.GetType(), nameof(BackupHandler.RestoreAsync)))
                            {
                                await handler.RestoreAsync(CurrentJob.AppId, reader);
                            }

                            Log($"Restored {handler.Name}");
                        }

                        foreach (var handler in handlers)
                        {
                            using (Profiler.TraceMethod(handler.GetType(), nameof(BackupHandler.CompleteRestoreAsync)))
                            {
                                await handler.CompleteRestoreAsync(CurrentJob.AppId, reader);
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
                    if (ex is BackupRestoreException backupException)
                    {
                        Log(backupException.Message);
                    }
                    else
                    {
                        Log("Failed with internal error");
                    }

                    await CleanupAsync();

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

                    await WriteStateAsync();
                }
            }
        }

        private async Task AssignContributorAsync()
        {
            var actor = CurrentJob.Actor;

            if (string.Equals(actor?.Type, RefTokenType.Subject))
            {
                await commandBus.PublishAsync(new AssignContributor
                {
                    Actor = actor,
                    AppId = CurrentJob.AppId,
                    ContributorId = actor.Identifier,
                    IsRestore = true,
                    Role = Role.Owner
                });

                Log("Assigned current user.");
            }
            else
            {
                Log("Current user not assigned because restore was triggered by client.");
            }
        }

        private async Task CleanupAsync()
        {
            await Safe.DeleteAsync(backupArchiveLocation, CurrentJob.Id, log);

            if (CurrentJob.AppId != Guid.Empty)
            {
                foreach (var handler in handlers)
                {
                    await Safe.CleanupRestoreAsync(handler, CurrentJob.AppId, CurrentJob.Id, log);
                }
            }
        }

        private async Task DownloadAsync()
        {
            Log("Downloading Backup");

            await backupArchiveLocation.DownloadAsync(CurrentJob.Url, CurrentJob.Id);

            Log("Downloaded Backup");
        }

        private async Task ReadEventsAsync(BackupReader reader)
        {
            await reader.ReadEventsAsync(streamNameResolver, eventDataFormatter, async storedEvent =>
            {
                await HandleEventAsync(reader, storedEvent.Stream, storedEvent.Event);
            });

            Log($"Reading {reader.ReadEvents} events and {reader.ReadAttachments} attachments completed.", true);
        }

        private async Task HandleEventAsync(BackupReader reader, string stream, Envelope<IEvent> @event)
        {
            if (@event.Payload is SquidexEvent squidexEvent)
            {
                squidexEvent.Actor = CurrentJob.Actor;
            }

            if (@event.Payload is AppCreated appCreated)
            {
                CurrentJob.AppId = appCreated.AppId.Id;

                if (!string.IsNullOrWhiteSpace(CurrentJob.NewAppName))
                {
                    appCreated.Name = CurrentJob.NewAppName;
                }
            }

            if (@event.Payload is AppEvent appEvent && !string.IsNullOrWhiteSpace(CurrentJob.NewAppName))
            {
                appEvent.AppId = NamedId.Of(appEvent.AppId.Id, CurrentJob.NewAppName);
            }

            foreach (var handler in handlers)
            {
                if (!await handler.RestoreEventAsync(@event, CurrentJob.AppId, reader, CurrentJob.Actor))
                {
                    return;
                }
            }

            var eventData = eventDataFormatter.ToEventData(@event, @event.Headers.CommitId());
            var eventCommit = new List<EventData> { eventData };

            await eventStore.AppendAsync(Guid.NewGuid(), stream, eventCommit);

            Log($"Read {reader.ReadEvents} events and {reader.ReadAttachments} attachments.", true);
        }

        private void Log(string message, bool replace = false)
        {
            if (replace && CurrentJob.Log.Count > 0)
            {
                CurrentJob.Log[CurrentJob.Log.Count - 1] = $"{clock.GetCurrentInstant()}: {message}";
            }
            else
            {
                CurrentJob.Log.Add($"{clock.GetCurrentInstant()}: {message}");
            }
        }

        public Task<J<IRestoreJob>> GetJobAsync()
        {
            return Task.FromResult<J<IRestoreJob>>(CurrentJob);
        }
    }
}
