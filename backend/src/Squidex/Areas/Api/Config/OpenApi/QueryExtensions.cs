// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using NJsonSchema;
using NSwag;

namespace Squidex.Areas.Api.Config.OpenApi
{
    public static class QueryExtensions
    {
        public static void AddQuery(this OpenApiOperation operation, bool supportSearch)
        {
            var stringSchema = new JsonSchema
            {
                Type = JsonObjectType.String
            };

            var numberSchema = new JsonSchema
            {
                Type = JsonObjectType.Number
            };

            void AddQuery(OpenApiParameter parameter)
            {
                if (operation.Parameters.Any(x => x.Name == parameter.Name && x.Kind == OpenApiParameterKind.Query))
                {
                    return;
                }

                parameter.Kind = OpenApiParameterKind.Query;

                operation.Parameters.Add(parameter);
            }

            if (supportSearch)
            {
                AddQuery(new OpenApiParameter
                {
                    Schema = stringSchema,
                    Name = "$search",
                    Description = "Optional OData full text search."
                });
            }

            AddQuery(new OpenApiParameter
            {
                Schema = numberSchema,
                Name = "$top",
                Description = "Optional OData parameter to define the number of items to retrieve."
            });

            AddQuery(new OpenApiParameter
            {
                Schema = numberSchema,
                Name = "$skip",
                Description = "Optional OData parameter to skip items."
            });

            AddQuery(new OpenApiParameter
            {
                Schema = stringSchema,
                Name = "$orderby",
                Description = "Optional OData order definition to sort the result set."
            });

            AddQuery(new OpenApiParameter
            {
                Schema = stringSchema,
                Name = "$filter",
                Description = "Optional OData order definition to filter the result set."
            });

            AddQuery(new OpenApiParameter
            {
                Schema = stringSchema,
                Name = "q",
                Description = "JSON query as well formatted json string. Overrides all other query parameters, except 'ids'."
            });

            AddQuery(new OpenApiParameter
            {
                Schema = stringSchema,
                Name = "ids",
                Description = "Comma separated list of content items. Overrides all other query parameters."
            });
        }
    }
}
