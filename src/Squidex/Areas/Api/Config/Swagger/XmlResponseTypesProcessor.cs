// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NJsonSchema.Infrastructure;
using NSwag;
using NSwag.SwaggerGeneration.Processors;
using NSwag.SwaggerGeneration.Processors.Contexts;

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

            return true;
        }
    }
}