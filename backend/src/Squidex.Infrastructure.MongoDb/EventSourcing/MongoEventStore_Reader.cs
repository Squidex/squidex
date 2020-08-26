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
using MongoDB.Bson;
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
        private const int ReadBeforeSeconds = 1;
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

            return new PollingSubscription(this, subscriber, streamFilter, position);
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
                    var eventStreamOffset = (int)commit.EventStreamOffset;

                    var commitTimestamp = commit.Timestamp;
                    var commitOffset = 0;

                    foreach (var @event in commit.Events)
                    {
                        eventStreamOffset++;

                        var eventData = @event.ToEventData();
                        var eventToken = new StreamPosition(commitTimestamp, commitOffset, commit.Events.Length);

                        result.Add(new StoredEvent(streamName, eventToken, eventStreamOffset, eventData));
                    }
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
                    var eventStreamOffset = (int)commit.EventStreamOffset;

                    var commitTimestamp = commit.Timestamp;
                    var commitOffset = 0;

                    foreach (var @event in commit.Events)
                    {
                        eventStreamOffset++;

                        if (eventStreamOffset >= streamPosition)
                        {
                            var eventData = @event.ToEventData();
                            var eventPosition = new StreamPosition(commitTimestamp, commitOffset, commit.Events.Length);

                            result.Add(new StoredEvent(streamName, eventPosition, eventStreamOffset, eventData));
                        }
                    }
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
            var filterPredicate = CreateFilterExpression(property, value);

            return QueryAsync(callback, lastPosition, filterDefinition, filterPredicate, ct);
        }

        public Task QueryAsync(Func<StoredEvent, Task> callback, string? streamFilter = null, string? position = null, CancellationToken ct = default)
        {
            Guard.NotNull(callback, nameof(callback));

            StreamPosition lastPosition = position;

            var filterDefinition = CreateFilter(streamFilter, lastPosition, false);
            var filterPredicate = EmptyPredicate;

            return QueryAsync(callback, lastPosition, filterDefinition, filterPredicate, ct);
        }

        public async Task QueryAtLeastOnceAsync(Func<StoredEvent, Task> callback, string? streamFilter = null, string? position = null, CancellationToken ct = default)
        {
            Guard.NotNull(callback, nameof(callback));

            StreamPosition lastPosition = position;

            var filterDefinition = CreateFilter(streamFilter, lastPosition, true);
            var filterPredicate = EmptyPredicate;

            using (Profiler.TraceMethod<MongoEventStore>())
            {
                await Collection.Find(filterDefinition, options: Batching.Options).Sort(Sort.Ascending(TimestampField)).ForEachPipelineAsync(async commit =>
                {
                    foreach (var @event in Filtered(commit, StreamPosition.Empty, filterPredicate))
                    {
                        await callback(@event);
                    }
                });
            }
        }

        private async Task QueryAsync(Func<StoredEvent, Task> callback, StreamPosition position, EventFilter filter, EventPredicate predicate, CancellationToken ct = default)
        {
            using (Profiler.TraceMethod<MongoEventStore>())
            {
                await Collection.Find(filter, options: Batching.Options).Sort(Sort.Ascending(TimestampField)).ForEachPipelineAsync(async commit =>
                {
                    foreach (var @event in Filtered(commit, position, predicate))
                    {
                        await callback(@event);
                    }
                }, ct);
            }
        }

        private static IEnumerable<StoredEvent> Filtered(MongoEventCommit commit, StreamPosition lastPosition, EventPredicate predicate)
        {
            var eventStreamOffset = commit.EventStreamOffset;

            var commitTimestamp = commit.Timestamp;
            var commitOffset = 0;

            foreach (var @event in commit.Events)
            {
                eventStreamOffset++;

                if ((commitOffset > lastPosition.CommitOffset || commitTimestamp > lastPosition.Timestamp) && predicate(@event))
                {
                    var eventData = @event.ToEventData();
                    var eventPosition = new StreamPosition(commitTimestamp, commitOffset, commit.Events.Length);

                    yield return new StoredEvent(commit.EventStream, eventPosition, eventStreamOffset, eventData);
                }

                commitOffset++;
            }
        }

        private static EventFilter CreateFilter(string property, object value, StreamPosition streamPosition)
        {
            var filters = new List<EventFilter>(2)
            {
                PositionFilter(streamPosition, false),
                PropertyFilter(property, value)
            };

            return Filter.And(filters);
        }

        private static EventFilter CreateFilter(string? streamFilter, StreamPosition streamPosition, bool atLeastOnce)
        {
            var filters = new List<EventFilter>(2)
            {
                PositionFilter(streamPosition, atLeastOnce)
            };

            if (!StreamFilter.IsAll(streamFilter))
            {
                if (streamFilter.Contains("^"))
                {
                    filters.Add(Filter.Regex(EventStreamField, streamFilter));
                }
                else
                {
                    filters.Add(Filter.Eq(EventStreamField, streamFilter));
                }
            }

            return Filter.And(filters);
        }

        private static EventFilter PropertyFilter(string property, object value)
        {
            return Filter.Eq(CreateIndexPath(property), value);
        }

        private static EventFilter PositionFilter(StreamPosition streamPosition, bool atLeastOnce)
        {
            if (atLeastOnce)
            {
                var before = new BsonTimestamp(Math.Max(0, streamPosition.Timestamp.Timestamp - ReadBeforeSeconds), 0);

                return Filter.Gte(TimestampField, before);
            }
            else if (streamPosition.IsEndOfCommit)
            {
                return Filter.Gt(TimestampField, streamPosition.Timestamp);
            }
            else
            {
                return Filter.Gte(TimestampField, streamPosition.Timestamp);
            }
        }

        private static EventPredicate CreateFilterExpression(string? property, object? value)
        {
            if (!string.IsNullOrWhiteSpace(property))
            {
                var jsonValue = JsonValue.Create(value);

                return x => x.Headers.TryGetValue(property, out var p) && p.Equals(jsonValue);
            }
            else
            {
                return CreateEmptyFilterExpression();
            }
        }

        private static EventPredicate CreateEmptyFilterExpression()
        {
            return x => true;
        }

        private static string CreateIndexPath(string property)
        {
            return $"Events.Metadata.{property}";
        }
    }
}