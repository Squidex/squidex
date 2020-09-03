﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using MongoDB.Driver;

namespace Squidex.Infrastructure.EventSourcing
{
    internal static class Filtering
    {
        public static string CreateIndexPath(string property)
        {
            return $"Events.Metadata.{property}";
        }

        public static FilterDefinition<MongoEventCommit> ByPosition(StreamPosition streamPosition)
        {
            if (streamPosition.IsEndOfCommit)
            {
                return Builders<MongoEventCommit>.Filter.Gt(x => x.Timestamp, streamPosition.Timestamp);
            }
            else
            {
                return Builders<MongoEventCommit>.Filter.Gte(x => x.Timestamp, streamPosition.Timestamp);
            }
        }

        public static FilterDefinition<MongoEventCommit>? ByStream(string? streamFilter)
        {
            if (StreamFilter.IsAll(streamFilter))
            {
                return null;
            }

            if (streamFilter.Contains("^"))
            {
                return Builders<MongoEventCommit>.Filter.Regex(x => x.EventStream, streamFilter);
            }
            else
            {
                return Builders<MongoEventCommit>.Filter.Eq(x => x.EventStream, streamFilter);
            }
        }

        public static FilterDefinition<ChangeStreamDocument<MongoEventCommit>>? ByChangeInStream(string? streamFilter)
        {
            if (StreamFilter.IsAll(streamFilter))
            {
                return null;
            }

            if (streamFilter.Contains("^"))
            {
                return Builders<ChangeStreamDocument<MongoEventCommit>>.Filter.Regex(x => x.FullDocument.EventStream, streamFilter);
            }
            else
            {
                return Builders<ChangeStreamDocument<MongoEventCommit>>.Filter.Eq(x => x.FullDocument.EventStream, streamFilter);
            }
        }

        public static IEnumerable<StoredEvent> Filtered(this MongoEventCommit commit, StreamPosition lastPosition, EventPredicate predicate)
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

        public static IEnumerable<StoredEvent> Filtered(this MongoEventCommit commit, long streamPosition)
        {
            var eventStreamOffset = commit.EventStreamOffset;

            var commitTimestamp = commit.Timestamp;
            var commitOffset = 0;

            foreach (var @event in commit.Events)
            {
                eventStreamOffset++;

                if (eventStreamOffset >= streamPosition)
                {
                    var eventData = @event.ToEventData();
                    var eventPosition = new StreamPosition(commitTimestamp, commitOffset, commit.Events.Length);

                    yield return new StoredEvent(commit.EventStream, eventPosition, eventStreamOffset, eventData);
                }

                commitOffset++;
            }
        }
    }
}
