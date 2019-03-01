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
using Microsoft.Azure.Documents;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.EventSourcing
{
    public delegate bool EventPredicate(EventData data);

    public partial class CosmosDbEventStore : IEventStore, IInitializable
    {
        public IEventSubscription CreateSubscription(IEventSubscriber subscriber, string streamFilter = null, string position = null)
        {
            Guard.NotNull(subscriber, nameof(subscriber));

            ThrowIfDisposed();

            return new CosmosDbSubscription(this, subscriber, streamFilter, position);
        }

        public Task CreateIndexAsync(string property)
        {
            Guard.NotNullOrEmpty(property, nameof(property));

            ThrowIfDisposed();

            return Task.CompletedTask;
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

                    return TaskHelper.Done;
                });

                return result;
            }
        }

        public Task QueryAsync(Func<StoredEvent, Task> callback, string property, object value, string position = null, CancellationToken ct = default)
        {
            Guard.NotNull(callback, nameof(callback));
            Guard.NotNullOrEmpty(property, nameof(property));
            Guard.NotNull(value, nameof(value));

            ThrowIfDisposed();

            StreamPosition lastPosition = position;

            var filterDefinition = FilterBuilder.CreateByProperty(property, value, lastPosition);
            var filterExpression = FilterBuilder.CreateExpression(property, value);

            return QueryAsync(callback, lastPosition, filterDefinition, filterExpression, ct);
        }

        public Task QueryAsync(Func<StoredEvent, Task> callback, string streamFilter = null, string position = null, CancellationToken ct = default)
        {
            Guard.NotNull(callback, nameof(callback));

            ThrowIfDisposed();

            StreamPosition lastPosition = position;

            var filterDefinition = FilterBuilder.CreateByFilter(streamFilter, lastPosition);
            var filterExpression = FilterBuilder.CreateExpression(null, null);

            return QueryAsync(callback, lastPosition, filterDefinition, filterExpression, ct);
        }

        private async Task QueryAsync(Func<StoredEvent, Task> callback, StreamPosition lastPosition, SqlQuerySpec query, EventPredicate filterExpression, CancellationToken ct = default)
        {
            using (Profiler.TraceMethod<CosmosDbEventStore>())
            {
                await documentClient.QueryAsync(collectionUri, query, async commit =>
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