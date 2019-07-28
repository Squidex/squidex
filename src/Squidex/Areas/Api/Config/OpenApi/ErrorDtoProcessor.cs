﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using NJsonSchema;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using Squidex.ClientLibrary.Management;
using Squidex.Pipeline.OpenApi;

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

        private static void CleanupResponses(OpenApiOperation operation)
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

        private JsonSchema GetErrorSchema(DocumentProcessorContext context)
        {
            var errorType = typeof(ErrorDto);

            return context.SchemaGenerator.Generate(errorType);
        }
    }
}
