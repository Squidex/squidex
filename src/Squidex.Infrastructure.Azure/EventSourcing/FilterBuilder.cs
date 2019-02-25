// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Infrastructure.EventSourcing
{
    internal static class FilterBuilder
    {
        public const string Collection = "Events";

        public static readonly string CommitId = nameof(CosmosDbEventCommit.Id).ToCamelCase();
        public static readonly string EventsCountField = nameof(CosmosDbEventCommit.EventsCount).ToCamelCase();
        public static readonly string EventStreamOffsetField = nameof(CosmosDbEventCommit.EventStreamOffset).ToCamelCase();
        public static readonly string EventStreamField = nameof(CosmosDbEventCommit.EventStream).ToCamelCase();
        public static readonly string TimestampField = nameof(CosmosDbEventCommit.Timestamp).ToCamelCase();

        public static async Task QueryAsync(this DocumentClient documentClient, Uri collectionUri, SqlQuerySpec querySpec, Func<CosmosDbEventCommit, Task> handler, CancellationToken ct = default)
        {
            var query =
                documentClient.CreateDocumentQuery<CosmosDbEventCommit>(collectionUri, querySpec)
                    .AsDocumentQuery();

            using (query)
            {
                var result = new List<StoredEvent>();

                while (query.HasMoreResults && !ct.IsCancellationRequested)
                {
                    var commits = await query.ExecuteNextAsync<CosmosDbEventCommit>(ct);

                    foreach (var commit in commits)
                    {
                        await handler(commit);
                    }
                }
            }
        }

        public static SqlQuerySpec AllIds(string streamName)
        {
            var query =
                $"SELECT TOP 1 " +
                $"  e.{CommitId}," +
                $"  e.{EventsCountField} " +
                $"FROM {Collection} e " +
                $"WHERE " +
                $"    e.{EventStreamField} = @name " +
                $"ORDER BY e.{EventStreamOffsetField} DESC";

            var parameters = new SqlParameterCollection
            {
                new SqlParameter("@name", streamName)
            };

            return new SqlQuerySpec(query, parameters);
        }

        public static SqlQuerySpec LastPosition(string streamName)
        {
            var query =
                $"SELECT TOP 1 " +
                $"  e.{EventStreamOffsetField}," +
                $"  e.{EventsCountField} " +
                $"FROM {Collection} e " +
                $"WHERE " +
                $"    e.{EventStreamField} = @name " +
                $"ORDER BY e.{EventStreamOffsetField} DESC";

            var parameters = new SqlParameterCollection
            {
                new SqlParameter("@name", streamName)
            };

            return new SqlQuerySpec(query, parameters);
        }

        public static SqlQuerySpec ByStreamName(string streamName, long streamPosition = 0)
        {
            var query =
                $"SELECT * " +
                $"FROM {Collection} e " +
                $"WHERE " +
                $"    e.{EventStreamField} = @name " +
                $"AND e.{EventStreamOffsetField} >= @position " +
                $"ORDER BY e.{EventStreamOffsetField} ASC";

            var parameters = new SqlParameterCollection
            {
                new SqlParameter("@name", streamName),
                new SqlParameter("@position", streamPosition)
            };

            return new SqlQuerySpec(query, parameters);
        }

        public static SqlQuerySpec CreateByProperty(string property, object value, StreamPosition streamPosition)
        {
            var filters = new List<string>();

            var parameters = new SqlParameterCollection();

            filters.ForPosition(parameters, streamPosition);
            filters.ForProperty(parameters, property, value);

            return BuildQuery(filters, parameters);
        }

        public static SqlQuerySpec CreateByFilter(string streamFilter, StreamPosition streamPosition)
        {
            var filters = new List<string>();

            var parameters = new SqlParameterCollection();

            filters.ForPosition(parameters, streamPosition);
            filters.ForRegex(parameters, streamFilter);

            return BuildQuery(filters, parameters);
        }

        private static SqlQuerySpec BuildQuery(List<string> filters, SqlParameterCollection parameters)
        {
            var query = $"SELECT * FROM {Collection} e WHERE {string.Join(" AND ", filters)} ORDER BY e.{TimestampField}";

            return new SqlQuerySpec(query, parameters);
        }

        private static void ForProperty(this List<string> filters, SqlParameterCollection parameters, string property, object value)
        {
            filters.Add($"ARRAY_CONTAINS(e.events, {{ \"header\": {{ \"{property}\": @value }} }}, true)");

            parameters.Add(new SqlParameter("@value", value));
        }

        private static void ForRegex(this List<string> filters, SqlParameterCollection parameters, string streamFilter)
        {
            if (!string.IsNullOrWhiteSpace(streamFilter) && !string.Equals(streamFilter, ".*", StringComparison.OrdinalIgnoreCase))
            {
                if (streamFilter.Contains("^"))
                {
                    filters.Add($"STARTSWITH(e.{EventStreamField}, @filter)");
                }
                else
                {
                    filters.Add($"e.{EventStreamField} = @filter");
                }

                parameters.Add(new SqlParameter("@filter", streamFilter));
            }
        }

        private static void ForPosition(this List<string> filters, SqlParameterCollection parameters, StreamPosition streamPosition)
        {
            if (streamPosition.IsEndOfCommit)
            {
                filters.Add($"e.{TimestampField} > @time");
            }
            else
            {
                filters.Add($"e.{TimestampField} >= @time");
            }

            parameters.Add(new SqlParameter("@time", streamPosition.Timestamp));
        }

        public static EventPredicate CreateExpression(string property, object value)
        {
            if (!string.IsNullOrWhiteSpace(property))
            {
                var jsonValue = JsonValue.Create(value);

                return x => x.Headers.TryGetValue(property, out var p) && p.Equals(jsonValue);
            }
            else
            {
                return x => true;
            }
        }
    }
}
