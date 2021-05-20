﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using NodaTime;
using Squidex.Infrastructure.MongoDb;
using Squidex.Log;
using EventFilter = MongoDB.Driver.FilterDefinition<Squidex.Infrastructure.EventSourcing.MongoEventCommit>;

namespace Squidex.Infrastructure.EventSourcing
{
    public delegate bool EventPredicate(MongoEvent data);

    public partial class MongoEventStore : MongoRepositoryBase<MongoEventCommit>, IEventStore
    {
        private static readonly List<StoredEvent> EmptyEvents = new List<StoredEvent>();

        public IEventSubscription CreateSubscription(IEventSubscriber subscriber, string? streamFilter = null, string? position = null)
        {
            Guard.NotNull(subscriber, nameof(subscriber));

            if (CanUseChangeStreams)
            {
                return new MongoEventStoreSubscription(this, subscriber, streamFilter, position);
            }
            else
            {
                return new PollingSubscription(this, subscriber, streamFilter, position);
            }
        }

        public async Task<IReadOnlyList<StoredEvent>> QueryLatestAsync(string streamName, int count)
        {
            Guard.NotNullOrEmpty(streamName, nameof(streamName));

            if (count <= 0)
            {
                return EmptyEvents;
            }

            using (Profiler.TraceMethod<MongoEventStore>())
            {
                var commits =
                    await Collection.Find(
                            Filter.Eq(EventStreamField, streamName))
                        .Sort(Sort.Descending(TimestampField)).Limit(count).ToListAsync();

                var result = commits.Select(x => x.Filtered()).Reverse().SelectMany(x => x).TakeLast(count).ToList();

                return result;
            }
        }

        public async Task<IReadOnlyList<StoredEvent>> QueryAsync(string streamName, long streamPosition = 0)
        {
            Guard.NotNullOrEmpty(streamName, nameof(streamName));

            using (Profiler.TraceMethod<MongoEventStore>())
            {
                var commits =
                    await Collection.Find(
                        Filter.And(
                            Filter.Eq(EventStreamField, streamName),
                            Filter.Gte(EventStreamOffsetField, streamPosition - MaxCommitSize)))
                        .Sort(Sort.Ascending(TimestampField)).ToListAsync();

                var result = commits.SelectMany(x => x.Filtered(streamPosition)).ToList();

                return result;
            }
        }

        public async Task<IReadOnlyDictionary<string, IReadOnlyList<StoredEvent>>> QueryManyAsync(IEnumerable<string> streamNames)
        {
            Guard.NotNull(streamNames, nameof(streamNames));

            using (Profiler.TraceMethod<MongoEventStore>())
            {
                var position = EtagVersion.Empty;

                var commits =
                    await Collection.Find(
                        Filter.And(
                            Filter.In(EventStreamField, streamNames),
                            Filter.Gte(EventStreamOffsetField, position)))
                        .Sort(Sort.Ascending(TimestampField)).ToListAsync();

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
                Collection.Find(filterDefinition, options: Batching.Options)
                    .Limit(take).Sort(Sort.Descending(TimestampField).Ascending(EventStreamField));

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
                    .Limit(take).Sort(Sort.Ascending(TimestampField).Ascending(EventStreamField));

            var taken = 0;

            using (var cursor = await find.ToCursorAsync(ct))
            {
                while (taken < take && await cursor.MoveNextAsync(ct))
                {
                    foreach (var current in cursor.Current)
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

                        if (taken == take)
                        {
                            break;
                        }
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
}
