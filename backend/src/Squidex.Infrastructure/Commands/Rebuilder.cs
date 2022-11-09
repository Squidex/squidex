// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Squidex.Caching;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.Commands;

public class Rebuilder
{
    private readonly IDomainObjectFactory domainObjectFactory;
    private readonly ILocalCache localCache;
    private readonly IEventStore eventStore;
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<Rebuilder> log;

    public Rebuilder(
        IDomainObjectFactory domainObjectFactory,
        ILocalCache localCache,
        IEventStore eventStore,
        IServiceProvider serviceProvider,
        ILogger<Rebuilder> log)
    {
        this.eventStore = eventStore;
        this.serviceProvider = serviceProvider;
        this.domainObjectFactory = domainObjectFactory;
        this.localCache = localCache;
        this.log = log;
    }

    public virtual Task RebuildAsync<T, TState>(string filter, int batchSize,
        CancellationToken ct = default)
        where T : DomainObject<TState> where TState : class, IDomainState<TState>, new()
    {
        return RebuildAsync<T, TState>(filter, batchSize, 0, ct);
    }

    public virtual async Task RebuildAsync<T, TState>(string filter, int batchSize, double errorThreshold,
        CancellationToken ct = default)
        where T : DomainObject<TState> where TState : class, IDomainState<TState>, new()
    {
        await ClearAsync<TState>();

        var ids = eventStore.QueryAllAsync(filter, ct: ct).Select(x => x.Data.Headers.AggregateId());

        await InsertManyAsync<T, TState>(ids, batchSize, errorThreshold, ct);
    }

    public virtual Task InsertManyAsync<T, TState>(IEnumerable<DomainId> source, int batchSize,
        CancellationToken ct = default)
        where T : DomainObject<TState> where TState : class, IDomainState<TState>, new()
    {
        return InsertManyAsync<T, TState>(source, batchSize, 0, ct);
    }

    public virtual async Task InsertManyAsync<T, TState>(IEnumerable<DomainId> source, int batchSize, double errorThreshold = 0,
        CancellationToken ct = default)
        where T : DomainObject<TState> where TState : class, IDomainState<TState>, new()
    {
        Guard.NotNull(source);

        var ids = source.ToAsyncEnumerable();

        await InsertManyAsync<T, TState>(ids, batchSize, errorThreshold, ct);
    }

    private async Task InsertManyAsync<T, TState>(IAsyncEnumerable<DomainId> source, int batchSize, double errorThreshold,
        CancellationToken ct = default)
        where T : DomainObject<TState> where TState : class, IDomainState<TState>, new()
    {
        var store = serviceProvider.GetRequiredService<IStore<TState>>();

        var parallelism = Environment.ProcessorCount;

        var handledIds = new HashSet<DomainId>();
        var handlerErrors = 0;

        using (localCache.StartContext())
        {
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
                                var domainObject = domainObjectFactory.Create<T, TState>(id, context);

                                await domainObject.RebuildStateAsync(ct);
                            }
                            catch (DomainObjectNotFoundException)
                            {
                                return;
                            }
                            catch (Exception ex)
                            {
                                log.LogWarning(ex, "Found corrupt domain object of type {type} with ID {id}.", typeof(T), id);
                                Interlocked.Increment(ref handlerErrors);
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

            await workerBlock.Completion;
        }

        var errorRate = (double)handlerErrors / handledIds.Count;

        if (errorRate > errorThreshold)
        {
            ThrowHelper.InvalidOperationException($"Error rate of {errorRate} is above threshold {errorThreshold}.");
        }
    }

    private async Task ClearAsync<TState>() where TState : class, IDomainState<TState>, new()
    {
        var store = serviceProvider.GetRequiredService<IStore<TState>>();

        await store.ClearSnapshotsAsync();
    }
}
