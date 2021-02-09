// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using NJsonSchema;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Infrastructure.Queries.Json
{
    public static class QueryParser
    {
        public static ClrQuery Parse(this JsonSchema schema, string json, IJsonSerializer jsonSerializer)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new ClrQuery();
            }

            var query = ParseFromJson(json, jsonSerializer);

            return Convert(schema, query);
        }

        public static ClrQuery Convert(this JsonSchema schema, Query<IJsonValue> query)
        {
            if (query == null)
            {
                return new ClrQuery();
            }

            var result = SimpleMapper.Map(query, new ClrQuery());

            var errors = new List<string>();

            ConvertSorting(schema, result, errors);
            ConvertFilters(schema, result, errors, query);

            if (errors.Count > 0)
            {
                throw new ValidationException(errors.Select(BuildError).ToList());
            }

            return result;
        }

        private static ValidationError BuildError(string message)
        {
            var error = T.Get("exception.invalidJsonQuery", new { message });

            return new ValidationError(error);
        }

        private static void ConvertFilters(JsonSchema schema, ClrQuery result, List<string> errors, Query<IJsonValue> query)
        {
            if (query.Filter != null)
            {
                var filter = JsonFilterVisitor.Parse(query.Filter, schema, errors);

                if (filter != null)
                {
                    result.Filter = Optimizer<ClrValue>.Optimize(filter);
                }
            }
        }

        private static void ConvertSorting(JsonSchema schema, ClrQuery result, List<string> errors)
        {
            if (result.Sort != null)
            {
                foreach (var sorting in result.Sort)
                {
                    sorting.Path.TryGetProperty(schema, errors, out _);
                }
            }
        }

        public static Query<IJsonValue> ParseFromJson(string json, IJsonSerializer jsonSerializer)
        {
            try
            {
               return jsonSerializer.Deserialize<Query<IJsonValue>>(json);
            }
            catch (JsonException ex)
            {
                var error = T.Get("exception.invalidJsonQueryJson", new { message = ex.Message });

                throw new ValidationException(error);
            }
        }
    }
}
