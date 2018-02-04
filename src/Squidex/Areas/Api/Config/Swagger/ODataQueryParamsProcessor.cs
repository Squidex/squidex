// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using NJsonSchema;
using NSwag.SwaggerGeneration.Processors;
using NSwag.SwaggerGeneration.Processors.Contexts;
using Squidex.Infrastructure.Tasks;
using Squidex.Pipeline.Swagger;

namespace Squidex.Areas.Api.Config.Swagger
{
    public sealed class ODataQueryParamsProcessor : IOperationProcessor
    {
        private readonly string path;
        private readonly string entity;
        private readonly bool supportSearch;

        public ODataQueryParamsProcessor(string path, string entity, bool supportSearch)
        {
            this.path = path;
            this.entity = entity;
            this.supportSearch = supportSearch;
        }

        public Task<bool> ProcessAsync(OperationProcessorContext context)
        {
            if (context.OperationDescription.Path == path)
            {
                if (supportSearch)
                {
                    context.OperationDescription.Operation.AddQueryParameter("$search", JsonObjectType.String, "Optional OData full text search.");
                }

                context.OperationDescription.Operation.AddQueryParameter("$top", JsonObjectType.Number, $"Optional number of {entity} to take.");
                context.OperationDescription.Operation.AddQueryParameter("$skip", JsonObjectType.Number, $"Optional number of {entity} to skip.");
                context.OperationDescription.Operation.AddQueryParameter("$orderby", JsonObjectType.String, "Optional OData order definition.");
                context.OperationDescription.Operation.AddQueryParameter("$filter", JsonObjectType.String, "Optional OData filter definition.");
            }

            return TaskHelper.True;
        }
    }
}
