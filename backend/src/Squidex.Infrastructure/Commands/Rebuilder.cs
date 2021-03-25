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
using Microsoft.Extensions.DependencyInjection;
using Squidex.Caching;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;

namespace Squidex.Infrastructure.Commands
{
    public delegate Task IdSource(Func<DomainId, Task> add);

    public class Rebuilder
    {
        private readonly ILocalCache localCache;
        private readonly IEventStore eventStore;
        private readonly IServiceProvider serviceProvider;

        public Rebuilder(
            ILocalCache localCache,
            IEventStore eventStore,
            IServiceProvider serviceProvider)
        {
            Guard.NotNull(localCache, nameof(localCache));
            Guard.NotNull(serviceProvider, nameof(serviceProvider));
            Guard.NotNull(eventStore, nameof(eventStore));

            this.eventStore = eventStore;
            this.serviceProvider = serviceProvider;
            this.localCache = localCache;
        }

        public virtual async Task RebuildAsync<T, TState>(string filter, CancellationToken ct) where T : DomainObject<TState> where TState : class, IDomainState<TState>, new()
        {
            var store = serviceProvider.GetRequiredService<IStore<TState>>();

            await store.ClearAsync();

            await InsertManyAsync<T, TState>(store, async target =>
            {
                await eventStore.QueryAsync(async storedEvent =>
                {
                    var id = storedEvent.Data.Headers.AggregateId();

                    await target(id);
                }, filter, ct: ct);
            }, ct);
        }

        public virtual async Task InsertManyAsync<T, TState>(IEnumerable<DomainId> source, CancellationToken ct = default) where T : DomainObject<TState> where TState : class, IDomainState<TState>, new()
        {
            Guard.NotNull(source, nameof(source));

            var store = serviceProvider.GetRequiredService<IStore<TState>>();

            await InsertManyAsync<T, TState>(store, async target =>
            {
                foreach (var id in source)
                {
                    await target(id);
                }
            }, ct);
        }

        private async Task InsertManyAsync<T, TState>(IStore<TState> store, IdSource source, CancellationToken ct = default) where T : DomainObject<TState> where TState : class, IDomainState<TState>, new()
        {
            var objectFactory = ActivatorUtilities.CreateFactory(typeof(T), new[] { typeof(IPersistenceFactory<TState>) });

            var worker = new ActionBlock<DomainId[]>(async ids =>
            {
                await using (var context = store.WithBatchContext(typeof(TState)))
                {
                    var factory = new object[] { context };

                    foreach (var id in ids)
                    {
                        try
                        {
                            var domainObject = (T)objectFactory(serviceProvider, factory);

                            domainObject.Setup(id);

                            await domainObject.RebuildStateAsync();
                        }
                        catch (DomainObjectNotFoundException)
                        {
                            return;
                        }
                    }
                }
            },
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount * 2,
                MaxMessagesPerTask = 1,
                BoundedCapacity = Environment.ProcessorCount * 2
            });

            var batch = new BatchBlock<DomainId>(100, new GroupingDataflowBlockOptions
            {
                BoundedCapacity = 400
            });

            batch.LinkTo(worker, new DataflowLinkOptions
            {
                PropagateCompletion = true
            });

            var handledIds = new HashSet<DomainId>();

            using (localCache.StartContext())
            {
                await source(async id =>
                {
                    if (handledIds.Add(id))
                    {
                        await batch.SendAsync(id, ct);
                    }
                });

                batch.Complete();

                await worker.Completion;
            }
        }
    }
}