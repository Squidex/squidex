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
using EventFilter = MongoDB.Driver.FilterDefinition<Squidex.Infrastructure.EventSourcing.MongoEventCommit>;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Infrastructure.EventSourcing;

public delegate bool EventPredicate(MongoEvent data);

public partial class MongoEventStore : MongoRepositoryBase<MongoEventCommit>, IEventStore
{
    private static readonly List<StoredEvent> EmptyEvents = new List<StoredEvent>();

    public IEventSubscription CreateSubscription(IEventSubscriber<StoredEvent> subscriber, string? streamFilter = null, string? position = null)
    {
        Guard.NotNull(subscriber);

        if (CanUseChangeStreams)
        {
            return new MongoEventStoreSubscription(this, subscriber, streamFilter, position);
        }
        else
        {
            return new PollingSubscription(this, subscriber, streamFilter, position);
        }
    }

    public async Task<IReadOnlyList<StoredEvent>> QueryReverseAsync(string streamName, int count = int.MaxValue,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(streamName);

        if (count <= 0)
        {
            return EmptyEvents;
        }

        using (Telemetry.Activities.StartActivity("MongoEventStore/QueryLatestAsync"))
        {
            var filter = Filter.Eq(x => x.EventStream, streamName);

            var commits =
                await Collection.Find(filter).Sort(Sort.Descending(x => x.Timestamp)).Limit(count)
                    .ToListAsync(ct);

            var result = commits.Select(x => x.Filtered()).Reverse().SelectMany(x => x).TakeLast(count).ToList();

            return result;
        }
    }

    public async Task<IReadOnlyList<StoredEvent>> QueryAsync(string streamName, long streamPosition = 0,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(streamName);

        using (Telemetry.Activities.StartActivity("MongoEventStore/QueryAsync"))
        {
            var filter =
                Filter.And(
                    Filter.Eq(x => x.EventStream, streamName),
                    Filter.Gte(x => x.EventStreamOffset, streamPosition - MaxCommitSize));

            var commits =
                await Collection.Find(filter).Sort(Sort.Ascending(x => x.Timestamp))
                    .ToListAsync(ct);

            var result = commits.SelectMany(x => x.Filtered(streamPosition)).ToList();

            return result;
        }
    }

    public async Task<IReadOnlyDictionary<string, IReadOnlyList<StoredEvent>>> QueryManyAsync(IEnumerable<string> streamNames,
        CancellationToken ct = default)
    {
        Guard.NotNull(streamNames);

        using (Telemetry.Activities.StartActivity("MongoEventStore/QueryManyAsync"))
        {
            var position = EtagVersion.Empty;

            var filter =
                Filter.And(
                    Filter.In(x => x.EventStream, streamNames),
                    Filter.Gte(x => x.EventStreamOffset, position));

            var commits =
                await Collection.Find(filter).Sort(Sort.Ascending(x => x.Timestamp))
                    .ToListAsync(ct);

            var result = commits.GroupBy(x => x.EventStream)
                .ToDictionary(
                    x => x.Key,
                    x => (IReadOnlyList<StoredEvent>)x.SelectMany(y => y.Filtered(position)).ToList());

            return result;
        }
    }

    public async IAsyncEnumerable<StoredEvent> QueryAllReverseAsync(string? streamFilter = null, Instant timestamp = default, int take = int.MaxValue,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (take <= 0)
        {
            yield break;
        }

        StreamPosition lastPosition = timestamp;

        var filterDefinition = CreateFilter(streamFilter, lastPosition);

        var find =
            Collection.Find(filterDefinition, Batching.Options)
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

    public async IAsyncEnumerable<StoredEvent> QueryAllAsync(string? streamFilter = null, string? position = null, int take = int.MaxValue,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        StreamPosition lastPosition = position;

        var filterDefinition = CreateFilter(streamFilter, lastPosition);

        var find =
            Collection.Find(filterDefinition)
                .Limit(take).Sort(Sort.Ascending(x => x.Timestamp).Ascending(x => x.EventStream));

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

    private static EventFilter CreateFilter(string? streamFilter, StreamPosition streamPosition)
    {
        var byPosition = FilterExtensions.ByPosition(streamPosition);
        var byStream = FilterExtensions.ByStream(streamFilter);

        if (byStream != null)
        {
            return Filter.And(byPosition, byStream);
        }

        return byPosition;
    }
}
