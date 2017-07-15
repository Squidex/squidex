// ==========================================================================
//  MongoEventStore.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Timers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Infrastructure.Tasks;

// ReSharper disable InvertIf
// ReSharper disable ConvertIfStatementToConditionalTernaryExpression

namespace Squidex.Infrastructure.MongoDb.EventStore
{
    public class MongoEventStore : MongoRepositoryBase<MongoEventCommit>, IEventStore, IDisposable
    {
        private static readonly BsonTimestamp EmptyTimestamp = new BsonTimestamp(0);
        private static readonly FieldDefinition<MongoEventCommit, BsonTimestamp> TimestampField = Fields.Build(x => x.Timestamp);
        private static readonly FieldDefinition<MongoEventCommit, long> EventsCountField = Fields.Build(x => x.EventsCount);
        private static readonly FieldDefinition<MongoEventCommit, long> EventStreamOffsetField = Fields.Build(x => x.EventStreamOffset);
        private static readonly FieldDefinition<MongoEventCommit, string> EventStreamField = Fields.Build(x => x.EventStream);
        private readonly IEventNotifier notifier;
        private readonly CompletionTimer timer;
        private readonly ConcurrentQueue<(BsonDocument Document, TaskCompletionSource<bool> Completion)> pendingCommits = new ConcurrentQueue<(BsonDocument Document, TaskCompletionSource<bool> Completion)>();
        private readonly Lazy<IMongoCollection<BsonDocument>> plainCollection;

        public MongoEventStore(IMongoDatabase database, IEventNotifier notifier)
            : base(database)
        {
            Guard.NotNull(notifier, nameof(notifier));

            this.notifier = notifier;

            timer = new CompletionTimer(50, ct => WriteAsync());

            plainCollection = new Lazy<IMongoCollection<BsonDocument>>(() => Database.GetCollection<BsonDocument>(CollectionName()));
        }

        public void Dispose()
        {
            timer.Dispose();
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
            return collection.Indexes.CreateOneAsync(Index.Ascending(x => x.EventStreamOffset).Ascending(x => x.EventStream), new CreateIndexOptions { Unique = true });
        }

        public IEventSubscription CreateSubscription(string streamFilter = null, string position = null)
        {
            return new PollingSubscription(this, notifier, streamFilter, position);
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

            await Collection.Find(filter).Sort(Sort.Ascending(EventStreamField)).ForEachAsync(async commit =>
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

        private async Task WriteAsync()
        {
            while (true)
            {
                var commitsToInsert = new List<(BsonDocument Document, TaskCompletionSource<bool> Completion)>();

                while (pendingCommits.TryDequeue(out var commit))
                {
                    commitsToInsert.Add(commit);
                }

                var numCommits = commitsToInsert.Count;

                if (numCommits == 0)
                {
                    return;
                }

                try
                {
                    await plainCollection.Value.InsertManyAsync(commitsToInsert.Select(x => x.Document), new InsertManyOptions { IsOrdered = false });

                    notifier.NotifyEventsStored();

                    foreach (var commit in commitsToInsert)
                    {
                        commit.Completion.SetResult(true);
                    }
                }
                catch (MongoBulkWriteException ex)
                {
                    foreach (var error in ex.WriteErrors)
                    {
                        var commit = commitsToInsert[error.Index];

                        if (error.Category == ServerErrorCategory.DuplicateKey)
                        {
                            var streamName = commit.Document[nameof(MongoEventCommit.EventStream)].AsString;
                            var streamOffset = commit.Document[nameof(MongoEventCommit.EventStreamOffset)].AsInt64;

                            var currentVersion = await GetEventStreamOffset(streamName);

                            var exception = new WrongEventVersionException(currentVersion, streamOffset);

                            commit.Completion.SetException(exception);
                        }
                        else
                        {
                            commit.Completion.SetException(new MongoWriteException(ex.ConnectionId, error, ex.WriteConcernError, ex));
                        }
                    }
                }
                catch (Exception ex)
                {
                    foreach (var commit in commitsToInsert)
                    {
                        commit.Completion.SetException(ex);
                    }
                }
            }
        }

        public async Task AppendEventsAsync(Guid commitId, string streamName, int expectedVersion, ICollection<EventData> events)
        {
            Guard.NotNullOrEmpty(streamName, nameof(streamName));
            Guard.NotNull(events, nameof(events));

            var eventsCount = events.Count;

            if (eventsCount > 0)
            {
                var commitEvents = new MongoEvent[events.Count];

                var i = 0;

                foreach (var e in events)
                {
                    var mongoEvent = new MongoEvent { EventId = e.EventId, Metadata = e.Metadata, Payload = e.Payload, Type = e.Type };

                    commitEvents[i++] = mongoEvent;
                }

                var cts = new TaskCompletionSource<bool>();

                var document = new MongoEventCommit
                {
                    Id = commitId,
                    Events = commitEvents,
                    EventsCount = eventsCount,
                    EventStream = streamName,
                    EventStreamOffset = expectedVersion,
                    Timestamp = EmptyTimestamp
                }.ToBsonDocument();

                pendingCommits.Enqueue((document, cts));

                timer.Wakeup();

                await cts.Task;
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
    }
}