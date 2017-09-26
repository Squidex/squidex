﻿// ==========================================================================
//  MongoEventStore.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.CQRS.Events
{
    public class MongoEventStore : MongoRepositoryBase<MongoEventCommit>, IEventStore
    {
        private const long AnyVersion = long.MinValue;
        private const int MaxAttempts = 20;
        private static readonly BsonTimestamp EmptyTimestamp = new BsonTimestamp(0);
        private static readonly FieldDefinition<MongoEventCommit, BsonTimestamp> TimestampField = Fields.Build(x => x.Timestamp);
        private static readonly FieldDefinition<MongoEventCommit, long> EventsCountField = Fields.Build(x => x.EventsCount);
        private static readonly FieldDefinition<MongoEventCommit, long> EventStreamOffsetField = Fields.Build(x => x.EventStreamOffset);
        private static readonly FieldDefinition<MongoEventCommit, string> EventStreamField = Fields.Build(x => x.EventStream);
        private readonly IEventNotifier notifier;

        public MongoEventStore(IMongoDatabase database, IEventNotifier notifier)
            : base(database)
        {
            Guard.NotNull(notifier, nameof(notifier));

            this.notifier = notifier;
        }

        protected override string CollectionName()
        {
            return "Events";
        }

        protected override MongoCollectionSettings CollectionSettings()
        {
            return new MongoCollectionSettings { ReadPreference = ReadPreference.Primary, WriteConcern = WriteConcern.WMajority };
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoEventCommit> collection)
        {
            return Task.WhenAll(
                collection.Indexes.CreateOneAsync(Index.Ascending(x => x.Timestamp).Ascending(x => x.EventStream)),
                collection.Indexes.CreateOneAsync(Index.Ascending(x => x.EventStream).Descending(x => x.EventStreamOffset), new CreateIndexOptions { Unique = true }));
        }

        public IEventSubscription CreateSubscription()
        {
            return new PollingSubscription(this, notifier);
        }

        public async Task<IReadOnlyList<StoredEvent>> GetEventsAsync(string streamName)
        {
            var result = await Observable.Create<StoredEvent>((observer, ct) =>
            {
                return GetEventsAsync(storedEvent =>
                {
                    observer.OnNext(storedEvent);

                    return TaskHelper.Done;
                }, ct, streamName);
            }).ToList();

            return result.ToList();
        }

        public async Task GetEventsAsync(Func<StoredEvent, Task> callback, CancellationToken cancellationToken, string streamFilter = null, string position = null)
        {
            Guard.NotNull(callback, nameof(callback));

            StreamPosition lastPosition = position;

            var filter = CreateFilter(streamFilter, lastPosition);

            await Collection.Find(filter).Sort(Sort.Ascending(TimestampField)).ForEachAsync(async commit =>
            {
                var eventStreamOffset = (int)commit.EventStreamOffset;

                var commitTimestamp = commit.Timestamp;
                var commitOffset = 0;

                foreach (var e in commit.Events)
                {
                    eventStreamOffset++;

                    if (commitOffset > lastPosition.CommitOffset || commitTimestamp > lastPosition.Timestamp)
                    {
                        var eventData = new EventData { EventId = e.EventId, Metadata = e.Metadata, Payload = e.Payload, Type = e.Type };
                        var eventToken = new StreamPosition(commitTimestamp, commitOffset, commit.Events.Length);

                        await callback(new StoredEvent(eventToken, eventStreamOffset, eventData));

                        commitOffset++;
                    }
                }
            }, cancellationToken);
        }

        public Task AppendEventsAsync(Guid commitId, string streamName, ICollection<EventData> events)
        {
            return AppendEventsInternalAsync(commitId, streamName, AnyVersion, events);
        }

        public Task AppendEventsAsync(Guid commitId, string streamName, int expectedVersion, ICollection<EventData> events)
        {
            Guard.GreaterEquals(expectedVersion, -1, nameof(expectedVersion));

            return AppendEventsInternalAsync(commitId, streamName, expectedVersion, events);
        }

        private async Task AppendEventsInternalAsync(Guid commitId, string streamName, long expectedVersion, ICollection<EventData> events)
        {
            Guard.NotNullOrEmpty(streamName, nameof(streamName));
            Guard.NotNull(events, nameof(events));

            if (events.Count == 0)
            {
                return;
            }

            var currentVersion = await GetEventStreamOffset(streamName);

            if (expectedVersion != AnyVersion && expectedVersion != currentVersion)
            {
                throw new WrongEventVersionException(currentVersion, expectedVersion);
            }

            var commit = BuildCommit(commitId, streamName, expectedVersion >= -1 ? expectedVersion : currentVersion, events);

            for (var attempt = 0; attempt < MaxAttempts; attempt++)
            {
                try
                {
                    await Collection.InsertOneAsync(commit);

                    notifier.NotifyEventsStored(streamName);

                    return;
                }
                catch (MongoWriteException ex)
                {
                    if (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
                    {
                        currentVersion = await GetEventStreamOffset(streamName);

                        if (expectedVersion != AnyVersion)
                        {
                            throw new WrongEventVersionException(currentVersion, expectedVersion);
                        }
                        else if (attempt < MaxAttempts)
                        {
                            expectedVersion = currentVersion;
                        }
                        else
                        {
                            throw new TimeoutException("Could not acquire a free slot for the commit within the provided time.");
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        private async Task<long> GetEventStreamOffset(string streamName)
        {
            var document =
                await Collection.Find(Filter.Eq(EventStreamField, streamName))
                    .Project<BsonDocument>(Project
                        .Include(EventStreamOffsetField)
                        .Include(EventsCountField))
                    .Sort(Sort.Descending(EventStreamOffsetField)).Limit(1)
                    .FirstOrDefaultAsync();

            if (document != null)
            {
                return document[nameof(MongoEventCommit.EventStreamOffset)].ToInt64() + document[nameof(MongoEventCommit.EventsCount)].ToInt64();
            }

            return -1;
        }

        private static FilterDefinition<MongoEventCommit> CreateFilter(string streamFilter, StreamPosition streamPosition)
        {
            var filters = new List<FilterDefinition<MongoEventCommit>>();

            if (streamPosition.IsEndOfCommit)
            {
                filters.Add(Filter.Gt(TimestampField, streamPosition.Timestamp));
            }
            else
            {
                filters.Add(Filter.Gte(TimestampField, streamPosition.Timestamp));
            }

            if (!string.IsNullOrWhiteSpace(streamFilter) && !string.Equals(streamFilter, ".*", StringComparison.OrdinalIgnoreCase))
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

        private static MongoEventCommit BuildCommit(Guid commitId, string streamName, long expectedVersion, ICollection<EventData> events)
        {
            var commitEvents = new MongoEvent[events.Count];

            var i = 0;

            foreach (var e in events)
            {
                var mongoEvent = new MongoEvent { EventId = e.EventId, Metadata = e.Metadata, Payload = e.Payload, Type = e.Type };

                commitEvents[i++] = mongoEvent;
            }

            var mongoCommit = new MongoEventCommit
            {
                Id = commitId,
                Events = commitEvents,
                EventsCount = events.Count,
                EventStream = streamName,
                EventStreamOffset = expectedVersion,
                Timestamp = EmptyTimestamp
            };

            return mongoCommit;
        }
    }
}