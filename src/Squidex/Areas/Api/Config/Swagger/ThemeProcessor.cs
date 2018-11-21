// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NSwag.SwaggerGeneration.Processors;
using NSwag.SwaggerGeneration.Processors.Contexts;
using Squidex.Config;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Areas.Api.Config.Swagger
{
    public sealed class ThemeProcessor : IDocumentProcessor
    {
        private const string Background = "#3f83df";

        private readonly string url;

        public ThemeProcessor(IOptions<MyUrlsOptions> urlOptions)
        {
            url = urlOptions.Value.BuildUrl("images/logo-white.png", false);
        }

        public Task ProcessAsync(DocumentProcessorContext context)
        {
            context.Document.BasePath = Constants.ApiPrefix;

            context.Document.Info.ExtensionData = new Dictionary<string, object>
            {
                ["x-logo"] = new { url, backgroundColor = Background }
            };

            return TaskHelper.Done;
        }
    }
}
