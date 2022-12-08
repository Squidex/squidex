// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Driver;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Infrastructure.EventSourcing;

public partial class MongoEventStore
{
    private const int MaxCommitSize = 100;
    private const int MaxWriteAttempts = 20;
    private static readonly BsonTimestamp EmptyTimestamp = new BsonTimestamp(0);

    public Task DeleteStreamAsync(string streamName,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(streamName);

        return Collection.DeleteManyAsync(x => x.EventStream == streamName, ct);
    }

    public Task DeleteAsync(string streamFilter,
        CancellationToken ct = default)
    {
        Guard.NotNullOrEmpty(streamFilter);

        return Collection.DeleteManyAsync(FilterExtensions.ByStream(streamFilter), ct);
    }

    public Task AppendAsync(Guid commitId, string streamName, ICollection<EventData> events,
        CancellationToken ct = default)
    {
        return AppendAsync(commitId, streamName, EtagVersion.Any, events, ct);
    }

    public async Task AppendAsync(Guid commitId, string streamName, long expectedVersion, ICollection<EventData> events,
        CancellationToken ct = default)
    {
        Guard.NotEmpty(commitId);
        Guard.NotNullOrEmpty(streamName);
        Guard.NotNull(events);
        Guard.GreaterEquals(expectedVersion, EtagVersion.Any);

        using (Telemetry.Activities.StartActivity("MongoEventStore/AppendAsync"))
        {
            if (events.Count == 0)
            {
                return;
            }

            var currentVersion = await GetEventStreamOffsetAsync(streamName, ct);

            if (expectedVersion > EtagVersion.Any && expectedVersion != currentVersion)
            {
                throw new WrongEventVersionException(currentVersion, expectedVersion);
            }

            var commit = BuildCommit(commitId, streamName, expectedVersion >= -1 ? expectedVersion : currentVersion, events);

            for (var attempt = 1; attempt <= MaxWriteAttempts; attempt++)
            {
                try
                {
                    await Collection.InsertOneAsync(commit, cancellationToken: ct);

                    if (!CanUseChangeStreams)
                    {
                        notifier.NotifyEventsStored(streamName);
                    }

                    return;
                }
                catch (MongoWriteException ex)
                {
                    if (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
                    {
                        currentVersion = await GetEventStreamOffsetAsync(streamName, ct);

                        if (expectedVersion > EtagVersion.Any)
                        {
                            throw new WrongEventVersionException(currentVersion, expectedVersion);
                        }

                        if (attempt >= MaxWriteAttempts)
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

    public async Task AppendUnsafeAsync(IEnumerable<EventCommit> commits,
        CancellationToken ct = default)
    {
        Guard.NotNull(commits);

        using (Telemetry.Activities.StartActivity("MongoEventStore/AppendUnsafeAsync"))
        {
            var writes = new List<WriteModel<MongoEventCommit>>();

            foreach (var commit in commits)
            {
                var document = BuildCommit(commit.Id, commit.StreamName, commit.Offset, commit.Events);

                writes.Add(new InsertOneModel<MongoEventCommit>(document));
            }

            if (writes.Count > 0)
            {
                await Collection.BulkWriteAsync(writes, BulkUnordered, ct);
            }
        }
    }

    private async Task<long> GetEventStreamOffsetAsync(string streamName,
        CancellationToken ct = default)
    {
        var document =
            await Collection.Find(Filter.Eq(x => x.EventStream, streamName))
                .Project<BsonDocument>(Projection
                    .Include(x => x.EventStreamOffset)
                    .Include(x => x.EventsCount))
                .Sort(Sort.Descending(x => x.EventStreamOffset)).Limit(1)
                .FirstOrDefaultAsync(ct);

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
