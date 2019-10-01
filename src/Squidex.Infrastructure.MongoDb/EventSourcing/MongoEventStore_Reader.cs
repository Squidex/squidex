// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.MongoDb;
using EventFilter = MongoDB.Driver.FilterDefinition<Squidex.Infrastructure.EventSourcing.MongoEventCommit>;

namespace Squidex.Infrastructure.EventSourcing
{
    public delegate bool EventPredicate(EventData data);

    public partial class MongoEventStore : MongoRepositoryBase<MongoEventCommit>, IEventStore
    {
        public Task CreateIndexAsync(string property)
        {
            Guard.NotNullOrEmpty(property, nameof(property));

            return Collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoEventCommit>(Index.Ascending(CreateIndexPath(property))));
        }

        public IEventSubscription CreateSubscription(IEventSubscriber subscriber, string? streamFilter = null, string? position = null)
        {
            Guard.NotNull(subscriber, nameof(subscriber));

            return new PollingSubscription(this, subscriber, streamFilter, position);
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
                            var eventToken = new StreamPosition(commitTimestamp, commitOffset, commit.Events.Length);

                            result.Add(new StoredEvent(streamName, eventToken, eventStreamOffset, eventData));
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
            var filterExpression = CreateFilterExpression(property, value);

            return QueryAsync(callback, lastPosition, filterDefinition, filterExpression, ct);
        }

        public Task QueryAsync(Func<StoredEvent, Task> callback, string? streamFilter = null, string? position = null, CancellationToken ct = default)
        {
            Guard.NotNull(callback, nameof(callback));

            StreamPosition lastPosition = position;

            var filterDefinition = CreateFilter(streamFilter, lastPosition);
            var filterExpression = CreateFilterExpression(null, null);

            return QueryAsync(callback, lastPosition, filterDefinition, filterExpression, ct);
        }

        private async Task QueryAsync(Func<StoredEvent, Task> callback, StreamPosition lastPosition, EventFilter filterDefinition, EventPredicate filterExpression, CancellationToken ct = default)
        {
            using (Profiler.TraceMethod<MongoEventStore>())
            {
                await Collection.Find(filterDefinition, options: Batching.Options).Sort(Sort.Ascending(TimestampField)).ForEachPipelineAsync(async commit =>
                {
                    var eventStreamOffset = (int)commit.EventStreamOffset;

                    var commitTimestamp = commit.Timestamp;
                    var commitOffset = 0;

                    foreach (var @event in commit.Events)
                    {
                        eventStreamOffset++;

                        if (commitOffset > lastPosition.CommitOffset || commitTimestamp > lastPosition.Timestamp)
                        {
                            var eventData = @event.ToEventData();

                            if (filterExpression(eventData))
                            {
                                var eventToken = new StreamPosition(commitTimestamp, commitOffset, commit.Events.Length);

                                await callback(new StoredEvent(commit.EventStream, eventToken, eventStreamOffset, eventData));
                            }
                        }

                        commitOffset++;
                    }
                }, ct);
            }
        }

        private static EventFilter CreateFilter(string property, object value, StreamPosition streamPosition)
        {
            var filters = new List<EventFilter>();

            AppendByPosition(streamPosition, filters);
            AppendByProperty(property, value, filters);

            return Filter.And(filters);
        }

        private static EventFilter CreateFilter(string? streamFilter, StreamPosition streamPosition)
        {
            var filters = new List<EventFilter>();

            AppendByPosition(streamPosition, filters);
            AppendByStream(streamFilter, filters);

            return Filter.And(filters);
        }

        private static void AppendByProperty(string property, object value, List<EventFilter> filters)
        {
            filters.Add(Filter.Eq(CreateIndexPath(property), value));
        }

        private static void AppendByStream(string? streamFilter, List<EventFilter> filters)
        {
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
        }

        private static void AppendByPosition(StreamPosition streamPosition, List<EventFilter> filters)
        {
            if (streamPosition.IsEndOfCommit)
            {
                filters.Add(Filter.Gt(TimestampField, streamPosition.Timestamp));
            }
            else
            {
                filters.Add(Filter.Gte(TimestampField, streamPosition.Timestamp));
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
                return x => true;
            }
        }

        private static string CreateIndexPath(string property)
        {
            return $"Events.Metadata.{property}";
        }
    }
}