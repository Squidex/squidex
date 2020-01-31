// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Infrastructure.EventSourcing
{
    internal static class FilterBuilder
    {
        public static SqlQuerySpec AllIds(string streamName)
        {
            var query =
                $"SELECT TOP 1 " +
                $"  e.id," +
                $"  e.eventsCount " +
                $"FROM {Constants.Collection} e " +
                $"WHERE " +
                $"    e.eventStream = @name " +
                $"ORDER BY e.eventStreamOffset DESC";

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
                $"  e.eventStreamOffset," +
                $"  e.eventsCount " +
                $"FROM {Constants.Collection} e " +
                $"WHERE " +
                $"    e.eventStream = @name " +
                $"ORDER BY e.eventStreamOffset DESC";

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
                $"FROM {Constants.Collection} e " +
                $"WHERE " +
                $"    e.eventStream = @name " +
                $"AND e.eventStreamOffset >= @position " +
                $"ORDER BY e.eventStreamOffset ASC";

            var parameters = new SqlParameterCollection
            {
                new SqlParameter("@name", streamName),
                new SqlParameter("@position", streamPosition)
            };

            return new SqlQuerySpec(query, parameters);
        }

        public static SqlQuerySpec ByStreamNameDesc(string streamName, long count)
        {
            var query =
                $"SELECT TOP {count}* " +
                $"FROM {Constants.Collection} e " +
                $"WHERE " +
                $"    e.eventStream = @name " +
                $"ORDER BY e.eventStreamOffset DESC";

            var parameters = new SqlParameterCollection
            {
                new SqlParameter("@name", streamName)
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

        public static SqlQuerySpec CreateByFilter(string? streamFilter, StreamPosition streamPosition)
        {
            var filters = new List<string>();

            var parameters = new SqlParameterCollection();

            filters.ForPosition(parameters, streamPosition);
            filters.ForRegex(parameters, streamFilter);

            return BuildQuery(filters, parameters);
        }

        private static SqlQuerySpec BuildQuery(IEnumerable<string> filters, SqlParameterCollection parameters)
        {
            var query = $"SELECT * FROM {Constants.Collection} e WHERE {string.Join(" AND ", filters)} ORDER BY e.timestamp";

            return new SqlQuerySpec(query, parameters);
        }

        private static void ForProperty(this ICollection<string> filters, SqlParameterCollection parameters, string property, object value)
        {
            filters.Add($"ARRAY_CONTAINS(e.events, {{ \"header\": {{ \"{property}\": @value }} }}, true)");

            parameters.Add(new SqlParameter("@value", value));
        }

        private static void ForRegex(this ICollection<string> filters, SqlParameterCollection parameters, string? streamFilter)
        {
            if (!StreamFilter.IsAll(streamFilter))
            {
                if (streamFilter.Contains("^"))
                {
                    filters.Add("STARTSWITH(e.eventStream, @filter)");
                }
                else
                {
                    filters.Add("e.eventStream = @filter");
                }

                parameters.Add(new SqlParameter("@filter", streamFilter));
            }
        }

        private static void ForPosition(this ICollection<string> filters, SqlParameterCollection parameters, StreamPosition streamPosition)
        {
            if (streamPosition.IsEndOfCommit)
            {
                filters.Add("e.timestamp > @time");
            }
            else
            {
                filters.Add("e.timestamp >= @time");
            }

            parameters.Add(new SqlParameter("@time", streamPosition.Timestamp));
        }

        public static EventPredicate CreateExpression(string? property, object? value)
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
