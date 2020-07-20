// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Queries;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentCommandMiddleware : GrainCommandMiddleware<ContentCommand, IContentGrain>
    {
        private readonly IContentEnricher contentEnricher;
        private readonly IContextProvider contextProvider;

        public ContentCommandMiddleware(IGrainFactory grainFactory, IContentEnricher contentEnricher, IContextProvider contextProvider)
            : base(grainFactory)
        {
            Guard.NotNull(contentEnricher, nameof(contentEnricher));
            Guard.NotNull(contextProvider, nameof(contextProvider));

            this.contentEnricher = contentEnricher;
            this.contextProvider = contextProvider;
        }

        public override async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            await base.HandleAsync(context, next);

            if (context.PlainResult is IContentEntity content && NotEnriched(context))
            {
                var enriched = await contentEnricher.EnrichAsync(content, true, contextProvider.Context);

                context.Complete(enriched);
            }
        }

        private static bool NotEnriched(CommandContext context)
        {
            return !(context.PlainResult is IEnrichedContentEntity);
        }
    }
}
