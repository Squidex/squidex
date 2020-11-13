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
using Squidex.Log;

namespace Squidex.Infrastructure.EventSourcing
{
    public delegate bool EventPredicate(EventData data);

    public partial class CosmosDbEventStore : IEventStore, IInitializable
    {
        private static readonly IReadOnlyList<StoredEvent> EmptyEvents = new List<StoredEvent>();

        public IEventSubscription CreateSubscription(IEventSubscriber subscriber, string? streamFilter = null, string? position = null)
        {
            Guard.NotNull(subscriber, nameof(subscriber));

            ThrowIfDisposed();

            return new CosmosDbSubscription(this, subscriber, streamFilter, position);
        }

        public async Task<IReadOnlyList<StoredEvent>> QueryLatestAsync(string streamName, int count)
        {
            Guard.NotNullOrEmpty(streamName, nameof(streamName));

            ThrowIfDisposed();

            if (count <= 0)
            {
                return EmptyEvents;
            }

            using (Profiler.TraceMethod<CosmosDbEventStore>())
            {
                var query = FilterBuilder.ByStreamNameDesc(streamName, count);

                var result = new List<StoredEvent>();

                await documentClient.QueryAsync(collectionUri, query, commit =>
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

                    return Task.CompletedTask;
                });

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

            ThrowIfDisposed();

            using (Profiler.TraceMethod<CosmosDbEventStore>())
            {
                var query = FilterBuilder.ByStreamName(streamName, streamPosition - MaxCommitSize);

                var result = new List<StoredEvent>();

                await documentClient.QueryAsync(collectionUri, query, commit =>
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

                    return Task.CompletedTask;
                });

                return result;
            }
        }

        public async Task QueryAsync(Func<StoredEvent, Task> callback, string? streamFilter = null, string? position = null, CancellationToken ct = default)
        {
            Guard.NotNull(callback, nameof(callback));

            ThrowIfDisposed();

            StreamPosition lastPosition = position;

            var filterDefinition = FilterBuilder.CreateByFilter(streamFilter, lastPosition);
            var filterExpression = FilterBuilder.CreateExpression(null, null);

            using (Profiler.TraceMethod<CosmosDbEventStore>())
            {
                await documentClient.QueryAsync(collectionUri, filterDefinition, async commit =>
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
    }
}