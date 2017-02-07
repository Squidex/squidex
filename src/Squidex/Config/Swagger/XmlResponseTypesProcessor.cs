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
using NJsonSchema;
using NJsonSchema.Generation;
using NJsonSchema.Infrastructure;
using NSwag;
using NSwag.CodeGeneration.SwaggerGenerators.WebApi.Processors;
using NSwag.CodeGeneration.SwaggerGenerators.WebApi.Processors.Contexts;
using Squidex.Controllers.Api;

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

                SwaggerResponse response;

                if (!operation.Responses.TryGetValue(statusCode, out response))
                {
                    response = new SwaggerResponse();

                    operation.Responses[statusCode] = response;
                }

                response.Description = match.Groups["Description"].Value;

                if (statusCode == "200")
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

            var errorType = typeof(ErrorDto);
            var errorSchema = JsonObjectTypeDescription.FromType(errorType, new Attribute[0], EnumHandling.String);

            var response = new SwaggerResponse { Description = "Operation failed." };

            response.Schema = await context.SwaggerGenerator.GenerateAndAppendSchemaFromTypeAsync(errorType, errorSchema.IsNullable, null);

            operation.Responses.Add("500", response);
        }

        private static void RemoveOkResponse(SwaggerOperation operation)
        {
            SwaggerResponse response;

            if (operation.Responses.TryGetValue("200", out response) &&
                response.Description != null &&
                response.Description.Contains("=>"))
            {
                operation.Responses.Remove("200");
            }
        }
    }
}