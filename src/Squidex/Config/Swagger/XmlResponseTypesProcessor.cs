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
using NJsonSchema.Generation;
using NJsonSchema.Infrastructure;
using NSwag;
using NSwag.AspNetCore;
using NSwag.SwaggerGeneration.Processors;
using NSwag.SwaggerGeneration.Processors.Contexts;
using Squidex.Controllers.Api;

// ReSharper disable UseObjectOrCollectionInitializer

namespace Squidex.Config.Swagger
{
    public sealed class XmlResponseTypesProcessor : IOperationProcessor
    {
        private static readonly Regex ResponseRegex = new Regex("(?<Code>[0-9]{3}) => (?<Description>.*)", RegexOptions.Compiled);

        private readonly SwaggerSettings swaggerSettings;

        public XmlResponseTypesProcessor(SwaggerSettings swaggerSettings)
        {
            this.swaggerSettings = swaggerSettings;
        }

        public async Task<bool> ProcessAsync(OperationProcessorContext context)
        {
            var hasOkResponse = false;

            var operation = context.OperationDescription.Operation;

            var returnsDescription = await context.MethodInfo.GetXmlDocumentationTagAsync("returns") ?? string.Empty;

            foreach (Match match in ResponseRegex.Matches(returnsDescription))
            {
                var statusCode = match.Groups["Code"].Value;

                if (!operation.Responses.TryGetValue(statusCode, out SwaggerResponse response))
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

        private async Task AddInternalErrorResponseAsync(OperationProcessorContext context, SwaggerOperation operation)
        {
            if (operation.Responses.ContainsKey("500"))
            {
                return;
            }

            var errorType = typeof(ErrorDto);
            var errorContract = swaggerSettings.ActualContractResolver.ResolveContract(errorType);
            var errorSchema = JsonObjectTypeDescription.FromType(errorType, errorContract, new Attribute[0], swaggerSettings.DefaultEnumHandling);

            var response = new SwaggerResponse { Description = "Operation failed." };

            response.Schema = await context.SwaggerGenerator.GenerateAndAppendSchemaFromTypeAsync(errorType, errorSchema.IsNullable, null);

            operation.Responses.Add("500", response);
        }

        private static void RemoveOkResponse(SwaggerOperation operation)
        {
            if (operation.Responses.TryGetValue("200", out SwaggerResponse response) &&
                response.Description != null &&
                response.Description.Contains("=>"))
            {
                operation.Responses.Remove("200");
            }
        }
    }
}