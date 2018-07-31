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
using Orleans;
using Squidex.Domain.Apps.Entities.Backup.Helpers;
using Squidex.Domain.Apps.Entities.Backup.State;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public sealed class RestoreGrain : GrainOfString, IRestoreGrain
    {
        private readonly IClock clock;
        private readonly IAssetStore assetStore;
        private readonly IEventDataFormatter eventDataFormatter;
        private readonly IGrainFactory grainFactory;
        private readonly ISemanticLog log;
        private readonly IEventStore eventStore;
        private readonly IBackupArchiveLocation backupArchiveLocation;
        private readonly IStore<string> store;
        private readonly IEnumerable<BackupHandler> handlers;
        private RefToken actor;
        private RestoreState state = new RestoreState();
        private IPersistence<RestoreState> persistence;

        private RestoreStateJob CurrentJob
        {
            get { return state.Job; }
        }

        public RestoreGrain(
            IAssetStore assetStore,
            IBackupArchiveLocation backupArchiveLocation,
            IClock clock,
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            IGrainFactory grainFactory,
            IEnumerable<BackupHandler> handlers,
            ISemanticLog log,
            IStore<string> store)
        {
            Guard.NotNull(assetStore, nameof(assetStore));
            Guard.NotNull(backupArchiveLocation, nameof(backupArchiveLocation));
            Guard.NotNull(clock, nameof(clock));
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(eventDataFormatter, nameof(eventDataFormatter));
            Guard.NotNull(grainFactory, nameof(grainFactory));
            Guard.NotNull(handlers, nameof(handlers));
            Guard.NotNull(store, nameof(store));
            Guard.NotNull(log, nameof(log));

            this.assetStore = assetStore;
            this.backupArchiveLocation = backupArchiveLocation;
            this.clock = clock;
            this.eventStore = eventStore;
            this.eventDataFormatter = eventDataFormatter;
            this.grainFactory = grainFactory;
            this.handlers = handlers;
            this.store = store;
            this.log = log;
        }

        public override async Task OnActivateAsync(string key)
        {
            actor = new RefToken("subject", key);

            persistence = store.WithSnapshots<RestoreState, string>(GetType(), key, s => state = s);

            await ReadAsync();

            RecoverAfterRestart();
        }

        private void RecoverAfterRestart()
        {
            RecoverAfterRestartAsync().Forget();
        }

        private async Task RecoverAfterRestartAsync()
        {
            if (CurrentJob?.Status == JobStatus.Started)
            {
                Log("Failed due application restart");

                CurrentJob.Status = JobStatus.Failed;

                await CleanupAsync();
                await WriteAsync();
            }
        }

        public Task RestoreAsync(Uri url)
        {
            Guard.NotNull(url, nameof(url));

            if (CurrentJob?.Status == JobStatus.Started)
            {
                throw new DomainException("A restore operation is already running.");
            }

            state.Job = new RestoreStateJob
            {
                Id = Guid.NewGuid(),
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
            using (Profiler.StartSession())
            {
                try
                {
                    Log("Started. The restore process has the following steps:");
                    Log("  * Download backup");
                    Log("  * Restore events and attachments.");
                    Log("  * Restore all objects like app, schemas and contents");
                    Log("  * Complete the restore operation for all objects");

                    log.LogInformation(w => w
                        .WriteProperty("action", "restore")
                        .WriteProperty("status", "started")
                        .WriteProperty("operationId", CurrentJob.Id.ToString())
                        .WriteProperty("url", CurrentJob.Url.ToString()));

                    using (Profiler.Trace("Download"))
                    {
                        await DownloadAsync();
                    }

                    using (var reader = await backupArchiveLocation.OpenArchiveAsync(CurrentJob.Id))
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

                    CurrentJob.Status = JobStatus.Completed;

                    Log("Completed, Yeah!");

                    log.LogInformation(w =>
                    {
                        w.WriteProperty("action", "restore");
                        w.WriteProperty("status", "completed");
                        w.WriteProperty("operationId", CurrentJob.Id.ToString());
                        w.WriteProperty("url", CurrentJob.Url.ToString());

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

                    await CleanupAsync(ex);

                    CurrentJob.Status = JobStatus.Failed;

                    log.LogError(ex, w =>
                    {
                        w.WriteProperty("action", "retore");
                        w.WriteProperty("status", "failed");
                        w.WriteProperty("operationId", CurrentJob.Id.ToString());
                        w.WriteProperty("url", CurrentJob.Url.ToString());

                        Profiler.Session?.Write(w);
                    });
                }
                finally
                {
                    CurrentJob.Stopped = clock.GetCurrentInstant();

                    await WriteAsync();
                }
            }
        }

        private async Task CleanupAsync(Exception exception = null)
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
            var isOwnerAdded = false;

            await reader.ReadEventsAsync(async (storedEvent) =>
            {
                var @event = eventDataFormatter.Parse(storedEvent.Data);

                if (@event.Payload is SquidexEvent squidexEvent)
                {
                    squidexEvent.Actor = actor;
                }

                if (@event.Payload is AppCreated appCreated)
                {
                    CurrentJob.AppId = appCreated.AppId.Id;
                }
                else if (@event.Payload is AppContributorAssigned appContributorAssigned)
                {
                    if (!isOwnerAdded)
                    {
                        isOwnerAdded = true;

                        appContributorAssigned.ContributorId = actor.Identifier;
                    }
                }

                foreach (var handler in handlers)
                {
                    await handler.RestoreEventAsync(@event, CurrentJob.AppId, reader);
                }

                var eventData = eventDataFormatter.ToEventData(@event, @event.Headers.CommitId());

                await eventStore.AppendAsync(Guid.NewGuid(), storedEvent.StreamName, new List<EventData> { eventData });

                Log($"Read {reader.ReadEvents} events and {reader.ReadAttachments} attachments.", true);
            });

            Log("Reading events completed.");
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

        private async Task ReadAsync()
        {
            await persistence.ReadAsync();
        }

        private async Task WriteAsync()
        {
            await persistence.WriteSnapshotAsync(state);
        }

        public Task<J<IRestoreJob>> GetJobAsync()
        {
            return Task.FromResult<J<IRestoreJob>>(CurrentJob);
        }
    }
}
