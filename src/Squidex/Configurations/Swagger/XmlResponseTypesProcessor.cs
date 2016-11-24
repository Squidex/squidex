// ==========================================================================
//  XmlResponseTypesProcessor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Text.RegularExpressions;
using NJsonSchema;
using NJsonSchema.Infrastructure;
using NSwag;
using NSwag.CodeGeneration.SwaggerGenerators.WebApi.Processors;
using NSwag.CodeGeneration.SwaggerGenerators.WebApi.Processors.Contexts;
using Squidex.Modules.Api;
// ReSharper disable UseObjectOrCollectionInitializer

namespace Squidex.Configurations.Swagger
{
    public sealed class XmlResponseTypesProcessor : IOperationProcessor
    {
        private static readonly Regex ResponseRegex = new Regex("(?<Code>[0-9]{3}) => (?<Description>.*)", RegexOptions.Compiled);

        public bool Process(OperationProcessorContext context)
        {
            var hasOkResponse = false;

            var operation = context.OperationDescription.Operation;

            var returnsDescription = context.MethodInfo.GetXmlDocumentation("returns") ?? string.Empty;

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
            
            AddInternalErrorResponse(context, operation);

            if (!hasOkResponse)
            {
                RemoveOkResponse(operation);
            }

            return true;
        }

        private static void AddInternalErrorResponse(OperationProcessorContext context, SwaggerOperation operation)
        {
            if (operation.Responses.ContainsKey("500"))
            {
                return;    
            }

            var errorType = typeof(ErrorDto);
            var errorSchema = JsonObjectTypeDescription.FromType(errorType, new Attribute[0], EnumHandling.String);

            var response = new SwaggerResponse { Description = "Operation failed." };

            response.Schema = context.SwaggerGenerator.GenerateAndAppendSchemaFromType(errorType, errorSchema.IsNullable, null);

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