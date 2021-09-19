// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using Namotion.Reflection;
using NJsonSchema;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using Squidex.Web;

namespace Squidex.Areas.Api.Config.OpenApi
{
    public sealed class ErrorDtoProcessor : IDocumentProcessor
    {
        public void Process(DocumentProcessorContext context)
        {
            var errorSchema = GetErrorSchema(context);

            foreach (var operation in context.Document.Paths.Values.SelectMany(x => x.Values))
            {
                AddErrorResponses(operation, errorSchema);

                CleanupResponses(operation);
            }
        }

        private static void AddErrorResponses(OpenApiOperation operation, JsonSchema errorSchema)
        {
            if (!operation.Responses.ContainsKey("500"))
            {
                const string description = "Operation failed.";

                var response = new OpenApiResponse { Description = description, Schema = errorSchema };

                operation.Responses["500"] = response;
            }

            foreach (var (code, response) in operation.Responses)
            {
                if (code != "404" && code.StartsWith("4", StringComparison.OrdinalIgnoreCase) && response.Schema == null)
                {
                    response.Schema = errorSchema;
                }
            }
        }

        private static void CleanupResponses(OpenApiOperation operation)
        {
            foreach (var (code, response) in operation.Responses.ToList())
            {
                if (string.IsNullOrWhiteSpace(response.Description) ||
                    response.Description?.Contains("=&gt;", StringComparison.Ordinal) == true ||
                    response.Description?.Contains("=>", StringComparison.Ordinal) == true)
                {
                    operation.Responses.Remove(code);
                }
            }
        }

        private static JsonSchema GetErrorSchema(DocumentProcessorContext context)
        {
            var errorType = typeof(ErrorDto).ToContextualType();

            return context.SchemaGenerator.GenerateWithReference<JsonSchema>(errorType, context.SchemaResolver);
        }
    }
}
