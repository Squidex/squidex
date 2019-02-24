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
using Microsoft.Azure.Documents.Linq;
using Squidex.Infrastructure.Log;

namespace Squidex.Infrastructure.EventSourcing
{
    public delegate bool EventPredicate(EventData data);

    public partial class CosmosDbEventStore : IEventStore, IInitializable
    {
        public IEventSubscription CreateSubscription(IEventSubscriber subscriber, string streamFilter, string position = null)
        {
            Guard.NotNull(subscriber, nameof(subscriber));
            Guard.NotNullOrEmpty(streamFilter, nameof(streamFilter));

            return new PollingSubscription(this, subscriber, streamFilter, position);
        }

        public Task CreateIndexAsync(string property)
        {
            return Task.CompletedTask;
        }

        public async Task<IReadOnlyList<StoredEvent>> QueryAsync(string streamName, long streamPosition = 0)
        {
            using (Profiler.TraceMethod<CosmosDbEventStore>())
            {
                var query =
                    documentClient.CreateDocumentQuery<CosmosDbEventCommit>(collectionUri,
                        FilterBuilder.ByStreamName(streamName, streamPosition));

                var documentQuery = query.AsDocumentQuery();

                var result = new List<StoredEvent>();

                while (documentQuery.HasMoreResults)
                {
                    var commits = await documentQuery.ExecuteNextAsync<CosmosDbEventCommit>();

                    foreach (var commit in commits)
                    {
                        var eventStreamOffset = (int)commit.EventStreamOffset;

                        var commitTimestamp = commit.Timestamp;
                        var commitOffset = 0;

                        foreach (var e in commit.Events)
                        {
                            eventStreamOffset++;

                            if (eventStreamOffset >= streamPosition)
                            {
                                var eventData = e.ToEventData();
                                var eventToken = new StreamPosition(commitTimestamp, commitOffset, commit.Events.Length);

                                result.Add(new StoredEvent(streamName, eventToken, eventStreamOffset, eventData));
                            }
                        }
                    }
                }

                return result;
            }
        }

        public Task QueryAsync(Func<StoredEvent, Task> callback, string property, object value, string position = null, CancellationToken ct = default)
        {
            Guard.NotNull(callback, nameof(callback));

            StreamPosition lastPosition = position;

            var filterDefinition = CreateFilter(property, value, lastPosition);
            var filterExpression = CreateFilterExpression(property, value);

            return QueryAsync(callback, lastPosition, filterDefinition, filterExpression, ct);
        }

        public Task QueryAsync(Func<StoredEvent, Task> callback, string streamFilter = null, string position = null, CancellationToken ct = default)
        {
            Guard.NotNull(callback, nameof(callback));

            StreamPosition lastPosition = position;

            var filterDefinition = CreateFilter(streamFilter, lastPosition);
            var filterExpression = CreateFilterExpression(null, null);

            return QueryAsync(callback, lastPosition, filterDefinition, filterExpression, ct);
        }

        private async Task QueryAsync(Func<StoredEvent, Task> callback, StreamPosition lastPosition, IQueryable<CosmosDbEventCommit> query, EventPredicate filterExpression, CancellationToken ct = default)
        {
            using (Profiler.TraceMethod<CosmosDbEventStore>())
            {
                var documentQuery = query.AsDocumentQuery();

                while (documentQuery.HasMoreResults && !ct.IsCancellationRequested)
                {
                    var commits = await documentQuery.ExecuteNextAsync<CosmosDbEventCommit>(ct);

                    foreach (var commit in commits)
                    {
                        var eventStreamOffset = (int)commit.EventStreamOffset;

                        var commitTimestamp = commit.Timestamp;
                        var commitOffset = 0;

                        foreach (var e in commit.Events)
                        {
                            eventStreamOffset++;

                            if (commitOffset > lastPosition.CommitOffset || commitTimestamp > lastPosition.Timestamp)
                            {
                                var eventData = e.ToEventData();

                                if (filterExpression(eventData))
                                {
                                    var eventToken = new StreamPosition(commitTimestamp, commitOffset, commit.Events.Length);

                                    await callback(new StoredEvent(commit.EventStream, eventToken, eventStreamOffset, eventData));
                                }
                            }

                            commitOffset++;
                        }
                    }
                }
            }
        }

        private IQueryable<CosmosDbEventCommit> CreateFilter(string property, object value, StreamPosition streamPosition)
        {
            var query = FilterBuilder.ByProperty(property, value, streamPosition);

            return documentClient.CreateDocumentQuery<CosmosDbEventCommit>(collectionUri, query);
        }

        private IQueryable<CosmosDbEventCommit> CreateFilter(string streamFilter, StreamPosition streamPosition)
        {
            var query = FilterBuilder.ByFilter(streamFilter, streamPosition);

            return documentClient.CreateDocumentQuery<CosmosDbEventCommit>(collectionUri, query);
        }

        private static EventPredicate CreateFilterExpression(string property, object value)
        {
            return FilterBuilder.CreateFilterExpression(property, value);
        }
    }
}