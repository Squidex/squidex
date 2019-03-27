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
        private readonly ILocalCache localCache;
        private readonly IStore<Guid> store;
        private readonly IEventStore eventStore;

        public Rebuilder(
            ILocalCache localCache,
            IStore<Guid> store,
            IEventStore eventStore)
        {
            this.eventStore = eventStore;
            this.localCache = localCache;
            this.store = store;
        }

        public Task RebuildAppsAsync(CancellationToken ct = default)
        {
            return RebuildManyAsync<AppState, AppGrain>("^app\\-", ct);
        }

        public Task RebuildSchemasAsync(CancellationToken ct = default)
        {
            return RebuildManyAsync<SchemaState, SchemaGrain>("^schema\\-", ct);
        }

        public Task RebuildRulesAsync(CancellationToken ct = default)
        {
            return RebuildManyAsync<RuleState, RuleGrain>("^rule\\-", ct);
        }

        public Task RebuildAssetsAsync(CancellationToken ct = default)
        {
            return RebuildManyAsync<AssetState, AssetGrain>("^asset\\-", ct);
        }

        public Task RebuildContentAsync(CancellationToken ct = default)
        {
            return RebuildManyAsync<ContentState, ContentGrain>("^content\\-", ct);
        }

        private async Task RebuildManyAsync<TState, TGrain>(string filter, CancellationToken ct) where TState : IDomainState<TState>, new()
        {
            var handledIds = new HashSet<Guid>();

            var worker = new ActionBlock<Guid>(async id =>
                {
                    try
                    {
                        var state = new TState
                        {
                            Version = EtagVersion.Empty
                        };

                        var persistence = store.WithSnapshotsAndEventSourcing(typeof(TGrain), id, (TState s) => state = s, e =>
                        {
                            state = state.Apply(e);

                            state.Version++;
                        });

                        await persistence.ReadAsync();
                        await persistence.WriteSnapshotAsync(state);
                    }
                    catch (DomainObjectNotFoundException)
                    {
                        return;
                    }
                },
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount * 2
                });

            using (localCache.StartContext())
            {
                await store.GetSnapshotStore<TState>().ClearAsync();

                await eventStore.QueryAsync(async storedEvent =>
                {
                    var id = storedEvent.Data.Headers.AggregateId();

                    if (handledIds.Add(id))
                    {
                        await worker.SendAsync(id);
                    }
                }, filter, ct: ct);

                worker.Complete();

                await worker.Completion;
            }
        }
    }
}