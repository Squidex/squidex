// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.MongoDb;
using EventFilter = MongoDB.Driver.FilterDefinition<Squidex.Infrastructure.EventSourcing.MongoEventCommit>;

namespace Squidex.Infrastructure.EventSourcing
{
    public delegate bool EventPredicate(MongoEvent data);

    public partial class MongoEventStore : MongoRepositoryBase<MongoEventCommit>, IEventStore
    {
        private static readonly List<StoredEvent> EmptyEvents = new List<StoredEvent>();
        private static readonly EventPredicate EmptyPredicate = x => true;

        public Task CreateIndexAsync(string property)
        {
            Guard.NotNullOrEmpty(property, nameof(property));

            return Collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoEventCommit>(
                    Index
                        .Ascending(CreateIndexPath(property))
                        .Ascending(TimestampField)));
        }

        public IEventSubscription CreateSubscription(IEventSubscriber subscriber, string? streamFilter = null, string? position = null)
        {
            Guard.NotNull(subscriber, nameof(subscriber));

            if (IsReplicaSet)
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

                var result = new List<StoredEvent>();

                foreach (var commit in commits)
                {
                    result.AddRange(commit.Filtered(long.MinValue));
                }

                IEnumerable<StoredEvent> ordered = result.OrderBy(x => x.EventStreamNumber);

                if (result.Count > count)
                {
                    ordered = ordered.Skip(result.Count - count);
                }

                return ordered.ToList();
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

                var result = new List<StoredEvent>();

                foreach (var commit in commits)
                {
                    result.AddRange(commit.Filtered(streamPosition));
                }

                return result;
            }
        }

        public Task QueryAsync(Func<StoredEvent, Task> callback, string property, object value, string? position = null, CancellationToken ct = default)
        {
            Guard.NotNull(callback, nameof(callback));
            Guard.NotNullOrEmpty(property, nameof(property));
            Guard.NotNull(value, nameof(value));

            StreamPosition lastPosition = position;

            var filterDefinition = CreateFilter(property, value, lastPosition);
            var filterPredicate = CreateFilterPredicate(property, value);

            return QueryAsync(callback, lastPosition, filterDefinition, filterPredicate, ct);
        }

        public Task QueryAsync(Func<StoredEvent, Task> callback, string? streamFilter = null, string? position = null, CancellationToken ct = default)
        {
            Guard.NotNull(callback, nameof(callback));

            StreamPosition lastPosition = position;

            var filterDefinition = CreateFilter(streamFilter, lastPosition);
            var filterPredicate = EmptyPredicate;

            return QueryAsync(callback, lastPosition, filterDefinition, filterPredicate, ct);
        }

        private async Task QueryAsync(Func<StoredEvent, Task> callback, StreamPosition position, EventFilter filter, EventPredicate predicate, CancellationToken ct = default)
        {
            using (Profiler.TraceMethod<MongoEventStore>())
            {
                await Collection.Find(filter, options: Batching.Options).Sort(Sort.Ascending(TimestampField)).ForEachPipelineAsync(async commit =>
                {
                    foreach (var @event in commit.Filtered(position, predicate))
                    {
                        await callback(@event);
                    }
                }, ct);
            }
        }

        private static EventFilter CreateFilter(string? streamFilter, StreamPosition streamPosition)
        {
            var byPosition = Filtering.ByPosition(streamPosition);
            var byStream = Filtering.ByStream(streamFilter);

            if (byStream != null)
            {
                return Filter.And(byPosition, byStream);
            }

            return byPosition;
        }

        private static EventFilter CreateFilter(string property, object value, StreamPosition streamPosition)
        {
            return Filter.And(Filtering.ByPosition(streamPosition), ByProperty(property, value));
        }

        private static EventPredicate CreateFilterPredicate(string? property, object? value)
        {
            if (!string.IsNullOrWhiteSpace(property))
            {
                var jsonValue = JsonValue.Create(value);

                return x => x.Headers.TryGetValue(property, out var p) && p.Equals(jsonValue);
            }
            else
            {
                return EmptyPredicate;
            }
        }

        private static EventFilter ByProperty(string property, object value)
        {
            return Builders<MongoEventCommit>.Filter.Eq(CreateIndexPath(property), value);
        }

        private static string CreateIndexPath(string property)
        {
            return $"Events.Metadata.{property}";
        }
    }
}