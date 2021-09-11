// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;

namespace Squidex.Infrastructure.EventSourcing
{
    internal static class FilterExtensions
    {
        private static readonly FeedOptions CrossPartition = new FeedOptions
        {
            EnableCrossPartitionQuery = true
        };

        public static async Task<T?> FirstOrDefaultAsync<T>(this IQueryable<T> queryable,
            CancellationToken ct = default)
        {
            var documentQuery = queryable.AsDocumentQuery();

            using (documentQuery)
            {
                if (documentQuery.HasMoreResults)
                {
                    var results = await documentQuery.ExecuteNextAsync<T>(ct);

                    return results.FirstOrDefault();
                }
            }

            return default!;
        }

        public static async IAsyncEnumerable<CosmosDbEventCommit> QueryAsync(this DocumentClient documentClient, Uri collectionUri, SqlQuerySpec querySpec,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            var query = documentClient.CreateDocumentQuery<CosmosDbEventCommit>(collectionUri, querySpec, CrossPartition);

            var documentQuery = query.AsDocumentQuery();

            using (documentQuery)
            {
                while (documentQuery.HasMoreResults && !ct.IsCancellationRequested)
                {
                    var items = await documentQuery.ExecuteNextAsync<CosmosDbEventCommit>(ct);

                    foreach (var item in items)
                    {
                        yield return item;
                    }
                }
            }
        }

        public static IEnumerable<StoredEvent> Filtered(this CosmosDbEventCommit commit, StreamPosition lastPosition)
        {
            var eventStreamOffset = commit.EventStreamOffset;

            var commitTimestamp = commit.Timestamp;
            var commitOffset = 0;

            foreach (var @event in commit.Events)
            {
                eventStreamOffset++;

                if (commitOffset > lastPosition.CommitOffset || commitTimestamp > lastPosition.Timestamp)
                {
                    var eventData = @event.ToEventData();
                    var eventPosition = new StreamPosition(commitTimestamp, commitOffset, commit.Events.Length);

                    yield return new StoredEvent(commit.EventStream, eventPosition, eventStreamOffset, eventData);
                }

                commitOffset++;
            }
        }

        public static IEnumerable<StoredEvent> Filtered(this CosmosDbEventCommit commit, long streamPosition = EtagVersion.Empty)
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
