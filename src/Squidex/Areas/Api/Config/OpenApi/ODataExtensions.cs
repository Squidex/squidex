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
                operation.AddQueryParameter("$search", JsonObjectType.String, "Optional OData full text search.");
            }

            operation.AddQueryParameter("$top", JsonObjectType.Number, $"Optional number of {entity} to take.");
            operation.AddQueryParameter("$skip", JsonObjectType.Number, $"Optional number of {entity} to skip.");
            operation.AddQueryParameter("$orderby", JsonObjectType.String, "Optional OData order definition.");
            operation.AddQueryParameter("$filter", JsonObjectType.String, "Optional OData filter definition.");
        }
    }
}
