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
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;

namespace Squidex.Infrastructure.Commands
{
    public delegate Task IdSource(Func<Guid, Task> add);

    public class Rebuilder
    {
        private readonly ILocalCache localCache;
        private readonly IStore<Guid> store;
        private readonly IEventStore eventStore;
        private readonly IServiceProvider serviceProvider;

        public Rebuilder(
            ILocalCache localCache,
            IStore<Guid> store,
            IEventStore eventStore,
            IServiceProvider serviceProvider)
        {
            Guard.NotNull(localCache);
            Guard.NotNull(store);
            Guard.NotNull(eventStore);

            this.eventStore = eventStore;
            this.serviceProvider = serviceProvider;
            this.localCache = localCache;
            this.store = store;
        }

        public Task RebuildAsync<T, TState>(string filter, CancellationToken ct) where T : DomainObjectBase<TState> where TState : class, IDomainState<TState>, new()
        {
            return RebuildAsync<T, TState>(async target =>
            {
                await eventStore.QueryAsync(async storedEvent =>
                {
                    var id = storedEvent.Data.Headers.AggregateId();

                    await target(id);
                }, filter, ct: ct);
            }, ct);
        }

        public virtual async Task RebuildAsync<T, TState>(IdSource source, CancellationToken ct = default) where T : DomainObjectBase<TState> where TState : class, IDomainState<TState>, new()
        {
            Guard.NotNull(source);

            await store.GetSnapshotStore<TState>().ClearAsync();

            await InsertManyAsync<T, TState>(source, ct);
        }

        public virtual async Task InsertManyAsync<T, TState>(IdSource source, CancellationToken ct = default) where T : DomainObjectBase<TState> where TState : class, IDomainState<TState>, new()
        {
            Guard.NotNull(source);

            var worker = new ActionBlock<Guid>(async id =>
            {
                try
                {
                    var domainObject = (T)serviceProvider.GetService(typeof(T));

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
                MaxDegreeOfParallelism = Environment.ProcessorCount * 2
            });

            var handledIds = new HashSet<Guid>();

            using (localCache.StartContext())
            {
                await source(new Func<Guid, Task>(async id =>
                {
                    if (handledIds.Add(id))
                    {
                        await worker.SendAsync(id, ct);
                    }
                }));

                worker.Complete();

                await worker.Completion;
            }
        }
    }
}