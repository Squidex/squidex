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
            await base.HandleAsync(context, next);

            if (context.Command is SquidexCommand command && context.PlainResult is IContentEntity content && NotEnriched(context))
            {
                var enriched = await contentEnricher.EnrichAsync(content, command.User);

                context.Complete(enriched);
            }
        }

        private static bool NotEnriched(CommandContext context)
        {
            return !(context.PlainResult is IEnrichedContentEntity);
        }
    }
}
