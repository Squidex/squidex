// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Queries;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject;

public sealed class ContentCommandMiddleware : CachingDomainObjectMiddleware<ContentCommand, ContentDomainObject, ContentDomainObject.State>
{
    private readonly IContentEnricher contentEnricher;
    private readonly IContextProvider contextProvider;

    public ContentCommandMiddleware(
        IDomainObjectFactory domainObjectFactory,
        IDomainObjectCache domainObjectCache,
        IContentEnricher contentEnricher,
        IContextProvider contextProvider)
        : base(domainObjectFactory, domainObjectCache)
    {
        this.contentEnricher = contentEnricher;
        this.contextProvider = contextProvider;
    }

    protected override async Task<object> EnrichResultAsync(CommandContext context, CommandResult result,
        CancellationToken ct)
    {
        var payload = await base.EnrichResultAsync(context, result, ct);

        if (payload is IContentEntity content and not IEnrichedContentEntity)
        {
            payload = await contentEnricher.EnrichAsync(content, true, contextProvider.Context, ct);
        }

        return payload;
    }
}
