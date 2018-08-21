// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.State;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.State;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.State;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;

namespace Migrate_01
{
    public sealed class Rebuilder
    {
        private readonly FieldRegistry fieldRegistry;
        private readonly ILocalCache localCache;
        private readonly IStore<Guid> store;
        private readonly IEventStore eventStore;

        public Rebuilder(
            FieldRegistry fieldRegistry,
            ILocalCache localCache,
            IStore<Guid> store,
            IEventStore eventStore)
        {
            this.fieldRegistry = fieldRegistry;
            this.eventStore = eventStore;
            this.localCache = localCache;
            this.store = store;
        }

        public async Task RebuildAppsAsync()
        {
            await store.GetSnapshotStore<AppState>().ClearAsync();

            await RebuildManyAsync("^app\\-", id => RebuildAsync<AppState, AppGrain>(id, (e, s) => s.Apply(e)));
        }

        public async Task RebuildSchemasAsync()
        {
            await store.GetSnapshotStore<SchemaState>().ClearAsync();

            await RebuildManyAsync("^schema\\-", id => RebuildAsync<SchemaState, SchemaGrain>(id, (e, s) => s.Apply(e, fieldRegistry)));
        }

        public async Task RebuildRulesAsync()
        {
            await store.GetSnapshotStore<RuleState>().ClearAsync();

            await RebuildManyAsync("^rule\\-", id => RebuildAsync<RuleState, RuleGrain>(id, (e, s) => s.Apply(e)));
        }

        public async Task RebuildAssetsAsync()
        {
            await store.GetSnapshotStore<AssetState>().ClearAsync();

            await RebuildManyAsync("^asset\\-", id => RebuildAsync<AssetState, AssetGrain>(id, (e, s) => s.Apply(e)));
        }

        public async Task RebuildContentAsync()
        {
            using (localCache.StartContext())
            {
                await store.GetSnapshotStore<ContentState>().ClearAsync();

                await RebuildManyAsync("^content\\-", async id =>
                {
                    try
                    {
                        await RebuildAsync<ContentState, ContentGrain>(id, (e, s) => s.Apply(e));
                    }
                    catch (DomainObjectNotFoundException)
                    {
                        return;
                    }
                });
            }
        }

        private async Task RebuildManyAsync(string filter, Func<Guid, Task> action)
        {
            var handledIds = new HashSet<Guid>();

            var worker = new ActionBlock<Guid>(action, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 32 });

            await eventStore.QueryAsync(async storedEvent =>
            {
                var id = Guid.Parse(storedEvent.Data.Metadata.Value<string>(CommonHeaders.AggregateId));

                if (handledIds.Add(id))
                {
                    await worker.SendAsync(id);
                }
            }, filter, ct: CancellationToken.None);

            worker.Complete();

            await worker.Completion;
        }

        private async Task RebuildAsync<TState, TGrain>(Guid key, Func<Envelope<IEvent>, TState, TState> func) where TState : IDomainState, new()
        {
            var state = new TState
            {
                Version = EtagVersion.Empty
            };

            var persistence = store.WithSnapshotsAndEventSourcing<TState, Guid>(typeof(TGrain), key, s => state = s, e =>
            {
                state = func(e, state);

                state.Version++;
            });

            await persistence.ReadAsync();
            await persistence.WriteSnapshotAsync(state);
        }
    }
}