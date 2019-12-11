﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure.Log;

namespace Squidex.Infrastructure.EventSourcing
{
    public partial class MongoEventStore
    {
        private const int MaxCommitSize = 100;
        private const int MaxWriteAttempts = 20;
        private static readonly BsonTimestamp EmptyTimestamp = new BsonTimestamp(0);

        public Task DeleteStreamAsync(string streamName)
        {
            Guard.NotNullOrEmpty(streamName);

            return Collection.DeleteManyAsync(x => x.EventStream == streamName);
        }

        public Task AppendAsync(Guid commitId, string streamName, ICollection<EventData> events)
        {
            return AppendAsync(commitId, streamName, EtagVersion.Any, events);
        }

        public async Task AppendAsync(Guid commitId, string streamName, long expectedVersion, ICollection<EventData> events)
        {
            Guard.NotEmpty(commitId);
            Guard.NotNullOrEmpty(streamName);
            Guard.NotNull(events);
            Guard.NotNullOrEmpty(streamName);
            Guard.NotNull(events);
            Guard.LessThan(events.Count, MaxCommitSize, "events.Count");
            Guard.GreaterEquals(expectedVersion, EtagVersion.Any);

            using (Profiler.TraceMethod<MongoEventStore>())
            {
                if (events.Count == 0)
                {
                    return;
                }

                var currentVersion = await GetEventStreamOffsetAsync(streamName);

                if (expectedVersion > EtagVersion.Any && expectedVersion != currentVersion)
                {
                    throw new WrongEventVersionException(currentVersion, expectedVersion);
                }

                var commit = BuildCommit(commitId, streamName, expectedVersion >= -1 ? expectedVersion : currentVersion, events);

                for (var attempt = 0; attempt < MaxWriteAttempts; attempt++)
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
                            currentVersion = await GetEventStreamOffsetAsync(streamName);

                            if (expectedVersion > EtagVersion.Any)
                            {
                                throw new WrongEventVersionException(currentVersion, expectedVersion);
                            }

                            if (attempt < MaxWriteAttempts)
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
        }

        private async Task<long> GetEventStreamOffsetAsync(string streamName)
        {
            var document =
                await Collection.Find(Filter.Eq(EventStreamField, streamName))
                    .Project<BsonDocument>(Projection
                        .Include(EventStreamOffsetField)
                        .Include(EventsCountField))
                    .Sort(Sort.Descending(EventStreamOffsetField)).Limit(1)
                    .FirstOrDefaultAsync();

            if (document != null)
            {
                return document[nameof(MongoEventCommit.EventStreamOffset)].ToInt64() + document[nameof(MongoEventCommit.EventsCount)].ToInt64();
            }

            return EtagVersion.Empty;
        }

        private static MongoEventCommit BuildCommit(Guid commitId, string streamName, long expectedVersion, ICollection<EventData> events)
        {
            var commitEvents = new MongoEvent[events.Count];

            var i = 0;

            foreach (var e in events)
            {
                var mongoEvent = MongoEvent.FromEventData(e);

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