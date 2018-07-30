// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Backup
{
    [Reentrant]
    public sealed class AppCleanerGrain : GrainOfString, IRemindable, IAppCleanerGrain
    {
        private readonly IGrainFactory grainFactory;
        private readonly IStore<Guid> store;
        private readonly IEventStore eventStore;
        private readonly IEnumerable<BackupHandler> handlers;
        private readonly ISemanticLog log;
        private IPersistence<State> persistence;
        private bool isCleaning;
        private State state = new State();

        [CollectionName("AppCleaner")]
        public sealed class State
        {
            public HashSet<Guid> Apps { get; set; } = new HashSet<Guid>();

            public HashSet<Guid> FailedApps { get; set; } = new HashSet<Guid>();
        }

        public AppCleanerGrain(IGrainFactory grainFactory, IEventStore eventStore, IStore<Guid> store, IEnumerable<BackupHandler> handlers, ISemanticLog log)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));
            Guard.NotNull(store, nameof(store));
            Guard.NotNull(handlers, nameof(handlers));
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(log, nameof(log));

            this.grainFactory = grainFactory;

            this.store = store;
            this.handlers = handlers;

            this.log = log;

            this.eventStore = eventStore;
        }

        public async override Task OnActivateAsync(string key)
        {
            await RegisterOrUpdateReminder("Default", TimeSpan.Zero, TimeSpan.FromMinutes(1));

            persistence = store.WithSnapshots<AppCleanerGrain, State, Guid>(Guid.Empty, s =>
            {
                state = s;
            });

            await ReadAsync();

            Clean();
        }

        public Task EnqueueAppAsync(Guid appId)
        {
            if (appId != Guid.Empty)
            {
                state.Apps.Add(appId);

                Clean();
            }

            return WriteAsync();
        }

        public Task ActivateAsync()
        {
            Clean();

            return TaskHelper.Done;
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            Clean();

            return TaskHelper.Done;
        }

        public Task<CleanerStatus> GetStatusAsync(Guid appId)
        {
            if (state.Apps.Contains(appId))
            {
                return Task.FromResult(CleanerStatus.Cleaning);
            }

            if (state.FailedApps.Contains(appId))
            {
                return Task.FromResult(CleanerStatus.Failed);
            }

            return Task.FromResult(CleanerStatus.Cleaned);
        }

        private void Clean()
        {
            CleanAsync().Forget();
        }

        private async Task CleanAsync()
        {
            if (isCleaning)
            {
                return;
            }

            isCleaning = true;
            try
            {
                foreach (var appId in state.Apps.ToList())
                {
                    await CleanupAppAsync(appId);
                }
            }
            finally
            {
                isCleaning = false;
            }
        }

        private async Task CleanupAppAsync(Guid appId)
        {
            using (Profiler.StartSession())
            {
                try
                {
                    log.LogInformation(w => w
                        .WriteProperty("action", "cleanApp")
                        .WriteProperty("status", "started")
                        .WriteProperty("appId", appId.ToString()));

                    await CleanupAppCoreAsync(appId);

                    log.LogInformation(w =>
                    {
                        w.WriteProperty("action", "cleanApp");
                        w.WriteProperty("status", "completed");
                        w.WriteProperty("appId", appId.ToString());

                        Profiler.Session?.Write(w);
                    });
                }
                catch (Exception ex)
                {
                    state.FailedApps.Add(appId);

                    log.LogError(ex, w =>
                    {
                        w.WriteProperty("action", "cleanApp");
                        w.WriteProperty("status", "failed");
                        w.WriteProperty("appId", appId.ToString());

                        Profiler.Session?.Write(w);
                    });
                }
                finally
                {
                    state.Apps.Remove(appId);

                    await WriteAsync();
                }
            }
        }

        private async Task CleanupAppCoreAsync(Guid appId)
        {
            using (Profiler.Trace("DeleteEvents"))
            {
                await eventStore.DeleteManyAsync("AppId", appId.ToString());
            }

            foreach (var handler in handlers)
            {
                using (Profiler.TraceMethod(handler.GetType(), nameof(BackupHandler.RemoveAsync)))
                {
                    await handler.RemoveAsync(appId);
                }
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
    }
}
