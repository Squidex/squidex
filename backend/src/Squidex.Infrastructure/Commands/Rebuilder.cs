// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Squidex.Caching;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;

namespace Squidex.Infrastructure.Commands
{
    public delegate Task IdSource(Func<DomainId, Task> add);

    public class Rebuilder
    {
        private readonly ILocalCache localCache;
        private readonly IStore<DomainId> store;
        private readonly IEventStore eventStore;
        private readonly IServiceProvider serviceProvider;

        public Rebuilder(
            ILocalCache localCache,
            IStore<DomainId> store,
            IEventStore eventStore,
            IServiceProvider serviceProvider)
        {
            Guard.NotNull(localCache, nameof(localCache));
            Guard.NotNull(store, nameof(store));
            Guard.NotNull(eventStore, nameof(eventStore));

            this.eventStore = eventStore;
            this.serviceProvider = serviceProvider;
            this.localCache = localCache;
            this.store = store;
        }

        public virtual async Task RebuildAsync<T, TState>(string filter, CancellationToken ct) where T : DomainObjectBase<TState> where TState : class, IDomainState<TState>, new()
        {
            await store.GetSnapshotStore<TState>().ClearAsync();

            await InsertManyAsync<T, TState>(async target =>
            {
                await eventStore.QueryAsync(async storedEvent =>
                {
                    var id = storedEvent.Data.Headers.AggregateId();

                    await target(id);
                }, filter, ct: ct);
            }, ct);
        }

        public virtual async Task InsertManyAsync<T, TState>(IEnumerable<DomainId> source, CancellationToken ct = default) where T : DomainObjectBase<TState> where TState : class, IDomainState<TState>, new()
        {
            Guard.NotNull(source, nameof(source));

            await InsertManyAsync<T, TState>(async target =>
            {
                foreach (var id in source)
                {
                    await target(id);
                }
            }, ct);
        }

        private async Task InsertManyAsync<T, TState>(IdSource source, CancellationToken ct = default) where T : DomainObjectBase<TState> where TState : class, IDomainState<TState>, new()
        {
            var worker = new ActionBlock<DomainId>(async id =>
            {
                try
                {
                    var domainObject = (T)serviceProvider.GetService(typeof(T))!;

                    domainObject.Setup(id);

                    await domainObject.RebuildStateAsync();
                }
                catch (DomainObjectNotFoundException)
                {
                    return;
                }
            },
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount * 4
            });

            var handledIds = new HashSet<DomainId>();

            using (localCache.StartContext())
            {
                await source(async id =>
                {
                    if (handledIds.Add(id))
                    {
                        await worker.SendAsync(id, ct);
                    }
                });

                worker.Complete();

                await worker.Completion;
            }
        }
    }
}