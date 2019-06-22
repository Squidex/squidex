// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using NJsonSchema;
using NSwag.SwaggerGeneration.Processors;
using NSwag.SwaggerGeneration.Processors.Contexts;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Areas.Api.Config.Swagger
{
    public sealed class FixProcessor : IOperationProcessor
    {
        private static readonly JsonSchema4 StringSchema = new JsonSchema4 { Type = JsonObjectType.String };

        public Task<bool> ProcessAsync(OperationProcessorContext context)
        {
            foreach (var parameter in context.Parameters.Values)
            {
                if (parameter.IsRequired && parameter.Schema != null && parameter.Schema.Type == JsonObjectType.String)
                {
                    parameter.Schema = StringSchema;
                }
            }

            return TaskHelper.True;
        }
    }
}
