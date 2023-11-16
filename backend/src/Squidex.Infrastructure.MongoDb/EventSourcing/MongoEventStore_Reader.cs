// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using MongoDB.Driver;
using NodaTime;
using Squidex.Infrastructure.MongoDb;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Infrastructure.EventSourcing;

public delegate bool EventPredicate(MongoEvent data);

public partial class MongoEventStore : MongoRepositoryBase<MongoEventCommit>, IEventStore
{
    private static readonly List<StoredEvent> EmptyEvents = [];

    public IEventSubscription CreateSubscription(IEventSubscriber<StoredEvent> subscriber, StreamFilter filter, string? position = null)
    {
        Guard.NotNull(subscriber);

        if (CanUseChangeStreams)
        {
            return new MongoEventStoreSubscription(this, subscriber, filter, position);
        }
        else
        {
            return new PollingSubscription(this, subscriber, filter, position);
        }
    }

    public async Task<IReadOnlyList<StoredEvent>> QueryStreamAsync(string streamName, long afterStreamPosition = EtagVersion.Empty,
        CancellationToken ct = default)
    {
        using (Telemetry.Activities.StartActivity("MongoEventStore/QueryAsync"))
        {
            var commits =
                await Collection.Find(CreateFilter(StreamFilter.Name(streamName), afterStreamPosition))
                    .ToListAsync(ct);

            var result = Convert(commits, afterStreamPosition);

            if ((commits.Count == 0 || commits[0].EventStreamOffset != afterStreamPosition) && afterStreamPosition > EtagVersion.Empty)
            {
                var filterBefore =
                    Filter.And(
                        FilterExtensions.ByStream(StreamFilter.Name(streamName)),
                        Filter.Lt(x => x.EventStreamOffset, afterStreamPosition));

                commits =
                    await Collection.Find(filterBefore).SortByDescending(x => x.EventStreamOffset).Limit(1)
                        .ToListAsync(ct);

                result = Convert(commits, afterStreamPosition).Concat(result).ToList();
            }

            return result;
        }
    }

    public async IAsyncEnumerable<StoredEvent> QueryAllReverseAsync(StreamFilter filter, Instant timestamp = default, int take = int.MaxValue,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (take <= 0)
        {
            yield break;
        }

        StreamPosition lastPosition = timestamp;

        var find =
            Collection.Find(CreateFilter(filter, lastPosition), Batching.Options)
                .Limit(take).Sort(Sort.Descending(x => x.Timestamp).Ascending(x => x.EventStream));

        var taken = 0;

        using (var cursor = await find.ToCursorAsync(ct))
        {
            while (taken < take && await cursor.MoveNextAsync(ct))
            {
                foreach (var current in cursor.Current)
                {
                    foreach (var @event in current.Filtered(lastPosition).Reverse())
                    {
                        yield return @event;

                        taken++;

                        if (taken == take)
                        {
                            break;
                        }
                    }

                    if (taken == take)
                    {
                        break;
                    }
                }
            }
        }
    }

    public async IAsyncEnumerable<StoredEvent> QueryAllAsync(StreamFilter filter, string? position = null, int take = int.MaxValue,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        StreamPosition lastPosition = position;

        var filterDefinition = CreateFilter(filter, lastPosition);

        var find =
            Collection.Find(filterDefinition).SortBy(x => x.Timestamp).ThenByDescending(x => x.EventStream)
                .Limit(take);

        var taken = 0;

        await foreach (var current in find.ToAsyncEnumerable(ct))
        {
            foreach (var @event in current.Filtered(lastPosition))
            {
                yield return @event;

                taken++;

                if (taken == take)
                {
                    break;
                }
            }
        }
    }

    private static IReadOnlyList<StoredEvent> Convert(IEnumerable<MongoEventCommit> commits)
    {
        return commits.OrderBy(x => x.EventStreamOffset).ThenBy(x => x.Timestamp).SelectMany(x => x.Filtered()).ToList();
    }

    private static IReadOnlyList<StoredEvent> Convert(IEnumerable<MongoEventCommit> commits, long streamPosition)
    {
        return commits.OrderBy(x => x.EventStreamOffset).ThenBy(x => x.Timestamp).SelectMany(x => x.Filtered(streamPosition)).ToList();
    }

    private static FilterDefinition<MongoEventCommit> CreateFilter(StreamFilter filter, StreamPosition streamPosition)
    {
        return Filter.And(FilterExtensions.ByPosition(streamPosition), FilterExtensions.ByStream(filter));
    }

    private static FilterDefinition<MongoEventCommit> CreateFilter(StreamFilter filter, long streamPosition)
    {
        return Filter.And(FilterExtensions.ByStream(filter), FilterExtensions.ByOffset(streamPosition));
    }
}
