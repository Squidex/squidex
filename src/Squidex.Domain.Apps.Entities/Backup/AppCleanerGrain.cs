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
using Squidex.Domain.Apps.Entities.Apps.State;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.State;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.State;
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
        private readonly IEnumerable<ICleanableAppStorage> storages;
        private readonly ISemanticLog log;
        private IPersistence<State> persistence;
        private bool isCleaning;
        private State state = new State();

        [CollectionName("Index_AppsByName")]
        public sealed class State
        {
            public HashSet<Guid> Apps { get; set; } = new HashSet<Guid>();

            public HashSet<Guid> PendingApps { get; set; } = new HashSet<Guid>();
        }

        public AppCleanerGrain(IGrainFactory grainFactory, IEventStore eventStore, IStore<Guid> store, IEnumerable<ICleanableAppStorage> storages, ISemanticLog log)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));
            Guard.NotNull(store, nameof(store));
            Guard.NotNull(storages, nameof(storages));
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(log, nameof(log));

            this.grainFactory = grainFactory;

            this.store = store;
            this.storages = storages;

            this.log = log;

            this.eventStore = eventStore;
        }

        public async override Task OnActivateAsync(string key)
        {
            await RegisterOrUpdateReminder("Default", TimeSpan.Zero, TimeSpan.FromMinutes(2));

            persistence = store.WithSnapshots<AppCleanerGrain, State, Guid>(Guid.Empty, s =>
            {
                state = s;
            });

            await persistence.ReadAsync();

            await CleanAsync();
        }

        public Task EnqueueAppAsync(Guid appId)
        {
            state.Apps.Add(appId);

            return persistence.WriteSnapshotAsync(state);
        }

        public Task ActivateAsync()
        {
            CleanAsync().Forget();

            return TaskHelper.Done;
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            CleanAsync().Forget();

            return TaskHelper.Done;
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
                    using (Profiler.StartSession())
                    {
                        try
                        {
                            log.LogInformation(w => w
                                .WriteProperty("action", "cleanApp")
                                .WriteProperty("status", "started")
                                .WriteProperty("appId", appId.ToString()));

                            await CleanAsync(appId);

                            state.Apps.Remove(appId);

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
                            state.PendingApps.Add(appId);

                            log.LogError(ex, w => w
                                .WriteProperty("action", "cleanApp")
                                .WriteProperty("appId", appId.ToString()));
                        }
                        finally
                        {
                            state.Apps.Remove(appId);

                            await persistence.WriteSnapshotAsync(state);
                        }
                    }
                }
            }
            finally
            {
                isCleaning = false;
            }
        }

        private async Task CleanAsync(Guid appId)
        {
            using (Profiler.Trace("DeleteEvents"))
            {
                await eventStore.DeleteManyAsync("AppId", appId);
            }

            using (Profiler.Trace("DeleteRules"))
            {
                var ruleIds = await grainFactory.GetGrain<IRulesByAppIndex>(appId).GetRuleIdsAsync();

                foreach (var ruleId in ruleIds)
                {
                    await store.RemoveSnapshotAsync<RuleState>(ruleId);
                }
            }

            using (Profiler.Trace("DeleteSchemas"))
            {
                var schemaIds = await grainFactory.GetGrain<ISchemasByAppIndex>(appId).GetSchemaIdsAsync();

                foreach (var schemaId in schemaIds)
                {
                    await store.RemoveSnapshotAsync<SchemaState>(schemaId);
                }
            }

            foreach (var storage in storages)
            {
                using (Profiler.Trace($"{storage.Name}.ClearAsync"))
                {
                    await storage.ClearAsync(appId);
                }
            }

            await store.RemoveSnapshotAsync<AppState>(appId);
        }

        private async Task DeleteAsync<TState>(Guid id)
        {
            await store.RemoveSnapshotAsync<TState>(id);
        }
    }
}
