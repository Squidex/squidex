// ==========================================================================
//  XmlResponseTypesProcessor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Text.RegularExpressions;
using NJsonSchema.Infrastructure;
using NSwag;
using NSwag.CodeGeneration.SwaggerGenerators.WebApi.Processors;
using NSwag.CodeGeneration.SwaggerGenerators.WebApi.Processors.Contexts;

namespace Squidex.Configurations.Swagger
{
    public sealed class XmlResponseTypesProcessor : IOperationProcessor
    {
        private static readonly Regex ResponseRegex = new Regex("(?<Code>[0-9]{3}) => (?<Description>.*)", RegexOptions.Compiled);

        public bool Process(OperationProcessorContext context)
        {
            var returnsDescription = context.MethodInfo.GetXmlDocumentation("returns") ?? string.Empty;

            foreach (Match match in ResponseRegex.Matches(returnsDescription))
            {
                var statusCode = match.Groups["Code"].Value;

                SwaggerResponse response;

                if (!context.OperationDescription.Operation.Responses.TryGetValue(statusCode, out response))
                {
                    response = new SwaggerResponse();

                    context.OperationDescription.Operation.Responses[statusCode] = response;
                }

                response.Description = match.Groups["Description"].Value;
            }

            return true;
        }
    }
}