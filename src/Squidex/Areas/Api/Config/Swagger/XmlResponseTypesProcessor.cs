// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NJsonSchema.Infrastructure;
using NSwag;
using NSwag.SwaggerGeneration.Processors;
using NSwag.SwaggerGeneration.Processors.Contexts;
using Squidex.Pipeline.Swagger;

#pragma warning disable RECS0033 // Convert 'if' to '||' expression

namespace Squidex.Areas.Api.Config.Swagger
{
    public sealed class XmlResponseTypesProcessor : IOperationProcessor
    {
        private static readonly Regex ResponseRegex = new Regex("(?<Code>[0-9]{3}) =&gt; (?<Description>.*)", RegexOptions.Compiled);

        public async Task<bool> ProcessAsync(OperationProcessorContext context)
        {
            var operation = context.OperationDescription.Operation;

            var returnsDescription = await context.MethodInfo.GetXmlDocumentationTagAsync("returns");

            if (!string.IsNullOrWhiteSpace(returnsDescription))
            {
                foreach (Match match in ResponseRegex.Matches(returnsDescription))
                {
                    var statusCode = match.Groups["Code"].Value;

                    if (!operation.Responses.TryGetValue(statusCode, out var response))
                    {
                        response = new SwaggerResponse();

                        operation.Responses[statusCode] = response;
                    }

                    var description = match.Groups["Description"].Value;

                    if (description.Contains("=&gt;"))
                    {
                        throw new InvalidOperationException("Description not formatted correcly.");
                    }

                    response.Description = description;
                }
            }

            await AddInternalErrorResponseAsync(context, operation);

            CleanupResponses(operation);

            return true;
        }

        private static async Task AddInternalErrorResponseAsync(OperationProcessorContext context, SwaggerOperation operation)
        {
            var errorSchema = await context.SchemaGenerator.GetErrorDtoSchemaAsync(context.SchemaResolver);

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
    }
}