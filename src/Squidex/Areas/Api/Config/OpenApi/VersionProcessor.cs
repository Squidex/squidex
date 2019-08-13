// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using Squidex.Web;

namespace Squidex.Areas.Api.Config.OpenApi
{
    public sealed class VersionProcessor : IDocumentProcessor
    {
        private readonly ExposedValues exposedValues;

        public VersionProcessor(ExposedValues exposedValues)
        {
            this.exposedValues = exposedValues;
        }

        public void Process(DocumentProcessorContext context)
        {
            if (exposedValues.TryGetValue("version", out var version))
            {
                context.Document.Info.Version = version;
            }
        }
    }
}
