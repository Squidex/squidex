// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Text.RegularExpressions;
using Namotion.Reflection;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Squidex.Areas.Api.Config.OpenApi
{
    public sealed class XmlResponseTypesProcessor : IOperationProcessor
    {
        private static readonly Regex ResponseRegex = new Regex("(?<Code>[0-9]{3}) =&gt; (?<Description>.*)", RegexOptions.Compiled);

        public bool Process(OperationProcessorContext context)
        {
            var operation = context.OperationDescription.Operation;

            var returnsDescription = context.MethodInfo.GetXmlDocsTag("returns");

            if (!string.IsNullOrWhiteSpace(returnsDescription))
            {
                foreach (var match in ResponseRegex.Matches(returnsDescription).OfType<Match>())
                {
                    var statusCode = match.Groups["Code"].Value;

                    if (!operation.Responses.TryGetValue(statusCode, out var response))
                    {
                        response = new OpenApiResponse();

                        operation.Responses[statusCode] = response;
                    }

                    var description = match.Groups["Description"].Value;

                    if (description.Contains("=&gt;", StringComparison.Ordinal))
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