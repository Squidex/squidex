// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NJsonSchema;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Infrastructure.Queries.Json
{
    public static class QueryParser
    {
        public static ClrQuery Parse(this JsonSchema schema, string json, IJsonSerializer jsonSerializer)
        {
            var query = ParseFromJson(json, jsonSerializer);

            var result = SimpleMapper.Map(query, new ClrQuery());

            var errors = new List<string>();

            if (result.Sort != null)
            {
                foreach (var sorting in result.Sort)
                {
                    sorting.Path.TryGetProperty(schema, errors, out _);
                }
            }

            if (query.Filter != null)
            {
                result.Filter = JsonFilterVisitor.Parse(query.Filter, schema, errors);
            }

            if (errors.Count > 0)
            {
                throw new ValidationException("Failed to parse json query", errors.Select(x => new ValidationError(x)).ToArray());
            }

            return result;
        }

        private static Query<IJsonValue> ParseFromJson(string json, IJsonSerializer jsonSerializer)
        {
            try
            {
               return jsonSerializer.Deserialize<Query<IJsonValue>>(json);
            }
            catch (JsonException ex)
            {
                throw new ValidationException("Failed to parse json query.", new ValidationError(ex.Message));
            }
        }
    }
}
