// ==========================================================================
//  XmlResponseTypesProcessor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NJsonSchema.Infrastructure;
using NSwag;
using NSwag.SwaggerGeneration.Processors;
using NSwag.SwaggerGeneration.Processors.Contexts;
using Squidex.Pipeline.Swagger;

// ReSharper disable UseObjectOrCollectionInitializer

namespace Squidex.Config.Swagger
{
    public sealed class XmlResponseTypesProcessor : IOperationProcessor
    {
        private static readonly Regex ResponseRegex = new Regex("(?<Code>[0-9]{3}) => (?<Description>.*)", RegexOptions.Compiled);

        public async Task<bool> ProcessAsync(OperationProcessorContext context)
        {
            var hasOkResponse = false;

            var operation = context.OperationDescription.Operation;

            var returnsDescription = await context.MethodInfo.GetXmlDocumentationTagAsync("returns") ?? string.Empty;

            foreach (Match match in ResponseRegex.Matches(returnsDescription))
            {
                var statusCode = match.Groups["Code"].Value;

                if (!operation.Responses.TryGetValue(statusCode, out var response))
                {
                    response = new SwaggerResponse();

                    operation.Responses[statusCode] = response;
                }

                response.Description = match.Groups["Description"].Value;

                if (string.Equals(statusCode, "200", StringComparison.OrdinalIgnoreCase))
                {
                    hasOkResponse = true;
                }
            }

            await AddInternalErrorResponseAsync(context, operation);

            if (!hasOkResponse)
            {
                RemoveOkResponse(operation);
            }

            return true;
        }

        private static async Task AddInternalErrorResponseAsync(OperationProcessorContext context, SwaggerOperation operation)
        {
            if (operation.Responses.ContainsKey("500"))
            {
                return;
            }

            operation.AddResponse("500", "Operation failed", await context.SwaggerGenerator.GetErrorDtoSchemaAsync());
        }

        private static void RemoveOkResponse(SwaggerOperation operation)
        {
            if (operation.Responses.TryGetValue("200", out var response) &&
                response.Description != null &&
                response.Description.Contains("=>"))
            {
                operation.Responses.Remove("200");
            }
        }
    }
}