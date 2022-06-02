// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Namotion.Reflection;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using Squidex.Infrastructure;

namespace Squidex.Areas.Api.Config.OpenApi
{
    public sealed class XmlResponseTypesProcessor : IOperationProcessor
    {
        private static readonly Regex ResponseRegex = new Regex("(?<Code>[0-9]{3})[\\s]*=((&gt;)|>)[\\s]*(?<Description>.*)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

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
                        ThrowHelper.InvalidOperationException("Description not formatted correcly.");
                        return default!;
                    }

                    response.Description = description;
                }
            }

            return true;
        }
    }
}
