// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Microsoft.Extensions.Options;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using Squidex.Web;

namespace Squidex.Areas.Api.Config.OpenApi
{
    public sealed class ThemeProcessor : IDocumentProcessor
    {
        private const string Background = "#3f83df";

        private readonly string url;

        public ThemeProcessor(IOptions<UrlsOptions> urlOptions)
        {
            url = urlOptions.Value.BuildUrl("images/logo-white.png", false);
        }

        public void Process(DocumentProcessorContext context)
        {
            context.Document.BasePath = $"{Constants.ApiPrefix}/";

            context.Document.Info.ExtensionData = new Dictionary<string, object>
            {
                ["x-logo"] = new { url, backgroundColor = Background }
            };
        }
    }
}
