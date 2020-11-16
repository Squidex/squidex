// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
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

        public static async Task<T?> FirstOrDefaultAsync<T>(this IQueryable<T> queryable, CancellationToken ct = default)
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

        public static Task QueryAsync(this DocumentClient documentClient, Uri collectionUri, SqlQuerySpec querySpec, Func<CosmosDbEventCommit, Task> handler, CancellationToken ct = default)
        {
            var query = documentClient.CreateDocumentQuery<CosmosDbEventCommit>(collectionUri, querySpec, CrossPartition);

            return query.QueryAsync(handler, ct);
        }

        public static async Task QueryAsync<T>(this IQueryable<T> queryable, Func<T, Task> handler, CancellationToken ct = default)
        {
            var documentQuery = queryable.AsDocumentQuery();

            using (documentQuery)
            {
                while (documentQuery.HasMoreResults && !ct.IsCancellationRequested)
                {
                    var items = await documentQuery.ExecuteNextAsync<T>(ct);

                    foreach (var item in items)
                    {
                        await handler(item);
                    }
                }
            }
        }
    }
}
