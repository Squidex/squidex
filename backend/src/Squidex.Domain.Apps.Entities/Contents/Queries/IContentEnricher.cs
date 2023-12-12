// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public interface IContentEnricher
{
    Task<EnrichedContent> EnrichAsync(Content content, bool cloneData, Context context,
        CancellationToken ct);

    Task<IReadOnlyList<EnrichedContent>> EnrichAsync(IEnumerable<Content> contents, Context context,
        CancellationToken ct);
}
