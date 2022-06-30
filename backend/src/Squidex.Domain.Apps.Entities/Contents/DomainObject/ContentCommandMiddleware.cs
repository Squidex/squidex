// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Queries;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject
{
    public sealed class ContentCommandMiddleware : ExecutableMiddleware<ContentCommand, ContentDomainObject>
    {
        private readonly IContentEnricher contentEnricher;
        private readonly IContextProvider contextProvider;

        public ContentCommandMiddleware(IServiceProvider serviceProvider,
            IContentEnricher contentEnricher, IContextProvider contextProvider)
            : base(serviceProvider)
        {
            this.contentEnricher = contentEnricher;
            this.contextProvider = contextProvider;
        }

        protected override async Task<object> EnrichResultAsync(CommandContext context, CommandResult result)
        {
            var payload = await base.EnrichResultAsync(context, result);

            if (payload is IContentEntity content && payload is not IEnrichedContentEntity)
            {
                payload = await contentEnricher.EnrichAsync(content, true, contextProvider.Context, default);
            }

            return payload;
        }
    }
}
