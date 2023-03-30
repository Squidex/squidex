// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;

namespace Squidex.Infrastructure.EventSourcing;

internal static class FilterExtensions
{
    public static FilterDefinition<MongoEventCommit> ByOffset(long streamPosition)
    {
        return Builders<MongoEventCommit>.Filter.Gte(x => x.EventStreamOffset, streamPosition);
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
            return Builders<MongoEventCommit>.Filter.Exists(x => x.EventStream, true);
        }

        if (streamFilter.Contains('^', StringComparison.Ordinal))
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

        if (streamFilter.Contains('^', StringComparison.Ordinal))
        {
            return Builders<ChangeStreamDocument<MongoEventCommit>>.Filter.Regex(x => x.FullDocument.EventStream, streamFilter);
        }
        else
        {
            return Builders<ChangeStreamDocument<MongoEventCommit>>.Filter.Eq(x => x.FullDocument.EventStream, streamFilter);
        }
    }

    public static IEnumerable<StoredEvent> Filtered(this MongoEventCommit commit, StreamPosition position)
    {
        var eventStreamOffset = commit.EventStreamOffset;

        var commitTimestamp = commit.Timestamp;
        var commitOffset = 0;

        foreach (var @event in commit.Events)
        {
            eventStreamOffset++;

            if (commitOffset > position.CommitOffset || commitTimestamp > position.Timestamp)
            {
                var eventData = @event.ToEventData();
                var eventPosition = new StreamPosition(commitTimestamp, commitOffset, commit.Events.Length);

                yield return new StoredEvent(commit.EventStream, eventPosition, eventStreamOffset, eventData);
            }

            commitOffset++;
        }
    }

    public static IEnumerable<StoredEvent> Filtered(this MongoEventCommit commit)
    {
        return commit.Filtered(EtagVersion.Empty);
    }

    public static IEnumerable<StoredEvent> Filtered(this MongoEventCommit commit, long position)
    {
        var eventStreamOffset = commit.EventStreamOffset;

        var commitTimestamp = commit.Timestamp;
        var commitOffset = 0;

        foreach (var @event in commit.Events)
        {
            eventStreamOffset++;

            if (eventStreamOffset > position)
            {
                var eventData = @event.ToEventData();
                var eventPosition = new StreamPosition(commitTimestamp, commitOffset, commit.Events.Length);

                yield return new StoredEvent(commit.EventStream, eventPosition, eventStreamOffset, eventData);
            }

            commitOffset++;
        }
    }
}
