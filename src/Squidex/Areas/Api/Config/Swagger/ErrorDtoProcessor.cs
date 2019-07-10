// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using NJsonSchema;
using NSwag;
using NSwag.SwaggerGeneration.Processors;
using NSwag.SwaggerGeneration.Processors.Contexts;
using Squidex.ClientLibrary.Management;
using Squidex.Pipeline.Swagger;

namespace Squidex.Areas.Api.Config.Swagger
{
    public sealed class ErrorDtoProcessor : IDocumentProcessor
    {
        public async Task ProcessAsync(DocumentProcessorContext context)
        {
            var errorSchema = await GetErrorSchemaAsync(context);

            foreach (var operation in context.Document.Paths.Values.SelectMany(x => x.Values))
            {
                AddErrorResponses(operation, errorSchema);

                CleanupResponses(operation);
            }
        }

        private static void AddErrorResponses(SwaggerOperation operation, JsonSchema4 errorSchema)
        {
            if (!operation.Responses.ContainsKey("500"))
            {
                operation.AddResponse("500", "Operation failed", errorSchema);
            }

            foreach (var (code, response) in operation.Responses)
            {
                if (code != "404" && code.StartsWith("4", StringComparison.OrdinalIgnoreCase) && response.Schema == null)
                {
                    response.Schema = errorSchema;
                }
            }
        }

        private static void CleanupResponses(SwaggerOperation operation)
        {
            foreach (var (code, response) in operation.Responses.ToList())
            {
                if (string.IsNullOrWhiteSpace(response.Description) ||
                    response.Description?.Contains("=&gt;") == true ||
                    response.Description?.Contains("=>") == true)
                {
                    operation.Responses.Remove(code);
                }
            }
        }

        private Task<JsonSchema4> GetErrorSchemaAsync(DocumentProcessorContext context)
        {
            var errorType = typeof(ErrorDto);

            return context.SchemaGenerator.GenerateWithReferenceAsync<JsonSchema4>(errorType, Enumerable.Empty<Attribute>(), context.SchemaResolver);
        }
    }
}
