// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public delegate Task<(Schema Schema, ResolvedComponents Components)> ProvideSchema(DomainId id);

public interface IContentEnricherStep
{
    // Steps that do not change the contents, but only the current request, must also run for cached results.
    bool RunOnCachedResults => false;

    Task EnrichAsync(Context context, IEnumerable<EnrichedContent> contents, ProvideSchema schemas,
        CancellationToken ct);

    Task EnrichAsync(Context context,
        CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
