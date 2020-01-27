// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema;
using NSwag;
using Squidex.Pipeline.OpenApi;

namespace Squidex.Areas.Api.Config.OpenApi
{
    public static class ODataExtensions
    {
        public static void AddOData(this OpenApiOperation operation, string entity, bool supportSearch)
        {
            if (supportSearch)
            {
                operation.AddQuery("$search", JsonObjectType.String, "Optional OData full text search.");
            }

            operation.AddQuery("$top", JsonObjectType.Integer, $"Optional number of {entity} to take.");
            operation.AddQuery("$skip", JsonObjectType.Integer, $"Optional number of {entity} to skip.");
            operation.AddQuery("$orderby", JsonObjectType.String, "Optional OData order definition.");
            operation.AddQuery("$filter", JsonObjectType.String, "Optional OData filter definition.");
        }
    }
}
