// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema;
using NSwag;

namespace Squidex.Areas.Api.Config.OpenApi
{
    public static class QueryExtensions
    {
        public static void AddQuery(this OpenApiOperation operation, bool supportSearch)
        {
            var @string = new JsonSchema
            {
                Type = JsonObjectType.String
            };

            var number = new JsonSchema
            {
                Type = JsonObjectType.String
            };

            if (supportSearch)
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Schema = @string,
                    Name = "$search",
                    Description = "Optional OData full text search.",
                    Kind = OpenApiParameterKind.Query
                });
            }

            operation.Parameters.Add(new OpenApiParameter
            {
                Schema = number,
                Name = "$top",
                Description = "Optional OData parameter to define the number of items to retrieve.",
                Kind = OpenApiParameterKind.Query
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Schema = number,
                Name = "$skip",
                Description = "Optional OData parameter to skip items.",
                Kind = OpenApiParameterKind.Query
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Schema = @string,
                Name = "$orderby",
                Description = "Optional OData order definition to sort the result set.",
                Kind = OpenApiParameterKind.Query
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Schema = @string,
                Name = "$filter",
                Description = "Optional OData order definition to filter the result set.",
                Kind = OpenApiParameterKind.Query
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Schema = @string,
                Name = "q",
                Description = "JSON query as well formatted json string. Overrides all other query parameters, except 'ids'.",
                Kind = OpenApiParameterKind.Query
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Schema = @string,
                Name = "ids",
                Description = "Comma separated list of content items. Overrides all other query parameters.",
                Kind = OpenApiParameterKind.Query
            });
        }
    }
}
