// ==========================================================================
//  AddODataQueryParams.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using NJsonSchema;
using NSwag.SwaggerGeneration.Processors;
using NSwag.SwaggerGeneration.Processors.Contexts;
using Squidex.Infrastructure.Tasks;
using Squidex.Pipeline.Swagger;

namespace Squidex.Docs
{
    public class AddODataQueryParams : IOperationProcessor
    {
        public Task<bool> ProcessAsync(OperationProcessorContext context)
        {
            if (context.OperationDescription.Path == "/apps/{app}/assets")
            {
                context.OperationDescription.Operation.AddQueryParameter("$top", JsonObjectType.Number, "Optional number of contents to take.");
                context.OperationDescription.Operation.AddQueryParameter("$skip", JsonObjectType.Number, "Optional number of contents to skip.");
                context.OperationDescription.Operation.AddQueryParameter("$search", JsonObjectType.String, "Optional OData full text search.");
                context.OperationDescription.Operation.AddQueryParameter("$orderby", JsonObjectType.String, "Optional OData order definition.");
                context.OperationDescription.Operation.AddQueryParameter("$filter", JsonObjectType.String, "Optional OData filter definition.");
            }

            return TaskHelper.True;
        }
    }
}
