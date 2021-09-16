// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Caching;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

#pragma warning disable RECS0108 // Warns about static fields in generic types

namespace Squidex.Infrastructure.Commands
{
    public class Rebuilder
    {
        private readonly ILocalCache localCache;
        private readonly IEventStore eventStore;
        private readonly IServiceProvider serviceProvider;

        private static class Factory<T, TState> where T : DomainObject<TState> where TState : class, IDomainState<TState>, new()
        {
            private static readonly ObjectFactory ObjectFactory = ActivatorUtilities.CreateFactory(typeof(T), new[] { typeof(IPersistenceFactory<TState>) });

            public static T Create(IServiceProvider serviceProvider, IPersistenceFactory<TState> persistenceFactory)
            {
                return (T)ObjectFactory(serviceProvider, new object[] { persistenceFactory });
            }
        }

        public Rebuilder(
            ILocalCache localCache,
            IEventStore eventStore,
            IServiceProvider serviceProvider)
        {
            this.eventStore = eventStore;
            this.serviceProvider = serviceProvider;
            this.localCache = localCache;
        }

        public virtual async Task RebuildAsync<T, TState>(string filter, int batchSize,
            CancellationToken ct = default)
            where T : DomainObject<TState> where TState : class, IDomainState<TState>, new()
        {
            await ClearAsync<TState>();

            var ids = eventStore.QueryAllAsync(filter, ct: ct).Select(x => x.Data.Headers.AggregateId());

            await InsertManyAsync<T, TState>(ids, batchSize, ct);
        }

        public virtual async Task InsertManyAsync<T, TState>(IEnumerable<DomainId> source, int batchSize,
            CancellationToken ct = default)
            where T : DomainObject<TState> where TState : class, IDomainState<TState>, new()
        {
            Guard.NotNull(source, nameof(source));

            var ids = source.ToAsyncEnumerable();

            await InsertManyAsync<T, TState>(ids, batchSize, ct);
        }

        private async Task InsertManyAsync<T, TState>(IAsyncEnumerable<DomainId> source, int batchSize,
            CancellationToken ct = default)
            where T : DomainObject<TState> where TState : class, IDomainState<TState>, new()
        {
            var store = serviceProvider.GetRequiredService<IStore<TState>>();

            var parallelism = Environment.ProcessorCount;

            var workerBlock = new ActionBlock<DomainId[]>(async ids =>
            {
                try
                {
                    await using (var context = store.WithBatchContext(typeof(T)))
                    {
                        await context.LoadAsync(ids);

                        foreach (var id in ids)
                        {
                            try
                            {
                                var domainObject = Factory<T, TState>.Create(serviceProvider, context);

                                domainObject.Setup(id);

                                await domainObject.RebuildStateAsync();
                            }
                            catch (DomainObjectNotFoundException)
                            {
                                return;
                            }
                        }
                    }
                }
                catch (OperationCanceledException ex)
                {
                    // Dataflow swallows operation cancelled exception.
                    throw new AggregateException(ex);
                }
            },
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = parallelism,
                MaxMessagesPerTask = 10,
                BoundedCapacity = parallelism
            });

            var batchBlock = new BatchBlock<DomainId>(batchSize, new GroupingDataflowBlockOptions
            {
                BoundedCapacity = batchSize
            });

            batchBlock.BidirectionalLinkTo(workerBlock);

            var handledIds = new HashSet<DomainId>();

            using (localCache.StartContext())
            {
                await foreach (var id in source.WithCancellation(ct))
                {
                    if (handledIds.Add(id))
                    {
                        if (!await batchBlock.SendAsync(id, ct))
                        {
                            break;
                        }
                    }
                }

                batchBlock.Complete();
            }

            await workerBlock.Completion;
        }

        private async Task ClearAsync<TState>() where TState : class, IDomainState<TState>, new()
        {
            var store = serviceProvider.GetRequiredService<IStore<TState>>();

            await store.ClearSnapshotsAsync();
        }
    }
}
