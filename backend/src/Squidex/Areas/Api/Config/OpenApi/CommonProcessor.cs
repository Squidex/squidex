// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using Squidex.Hosting;
using Squidex.Web;

namespace Squidex.Areas.Api.Config.OpenApi
{
    public sealed class CommonProcessor : IDocumentProcessor
    {
        private readonly string version;
        private readonly string logoBackground = "#3f83df";
        private readonly string logoUrl;

        private readonly OpenApiExternalDocumentation documentation = new OpenApiExternalDocumentation
        {
            Url = "https://docs.squidex.io"
        };

        public CommonProcessor(ExposedValues exposedValues, IUrlGenerator urlGenerator)
        {
            logoUrl = urlGenerator.BuildUrl("images/logo-white.png", false);

            if (!exposedValues.TryGetValue("version", out version!) || version == null)
            {
                version = "1.0";
            }
        }

        public void Process(DocumentProcessorContext context)
        {
            context.Document.BasePath = Constants.PrefixApi;

            context.Document.Info.Version = version;
            context.Document.Info.ExtensionData = new Dictionary<string, object>
            {
                ["x-logo"] = new { url = logoUrl, backgroundColor = logoBackground }
            };

            context.Document.ExternalDocumentation = documentation;
        }
    }
}
