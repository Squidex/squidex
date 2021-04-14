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

#pragma warning disable RECS0108 // Warns about static fields in generic types

namespace Squidex.Infrastructure.Commands
{
    public delegate Task IdSource(Func<DomainId, Task> add);

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

            await store.ClearSnapshotsAsync();

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
            var parallelism = Environment.ProcessorCount;

            const int BatchSize = 100;

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

            var batchBlock = new BatchBlock<DomainId>(BatchSize, new GroupingDataflowBlockOptions
            {
                BoundedCapacity = BatchSize
            });

            batchBlock.LinkTo(workerBlock, new DataflowLinkOptions
            {
                PropagateCompletion = true
            });

            var handledIds = new HashSet<DomainId>();

            using (localCache.StartContext())
            {
                await source(id =>
                {
                    if (handledIds.Add(id))
                    {
                        return batchBlock.SendAsync(id, ct);
                    }

                    return Task.CompletedTask;
                });

                batchBlock.Complete();

                await workerBlock.Completion;
            }
        }
    }
}