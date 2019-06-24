// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentCommandMiddleware : GrainCommandMiddleware<ContentCommand, IContentGrain>
    {
        private readonly IContentEnricher contentEnricher;

        public ContentCommandMiddleware(IGrainFactory grainFactory, IContentEnricher contentEnricher)
            : base(grainFactory)
        {
            Guard.NotNull(contentEnricher, nameof(contentEnricher));

            this.contentEnricher = contentEnricher;
        }

        public override async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            await HandleAsync(context, next);

            if (context.PlainResult is IContentEntity content)
            {
                var enriched = await contentEnricher.EnrichAsync(content);

                context.Complete(enriched);
            }
        }
    }
}
