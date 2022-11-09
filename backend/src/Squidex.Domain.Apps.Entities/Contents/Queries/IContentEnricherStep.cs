// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public delegate Task<(ISchemaEntity Schema, ResolvedComponents Components)> ProvideSchema(DomainId id);

public interface IContentEnricherStep
{
    Task EnrichAsync(Context context, IEnumerable<ContentEntity> contents, ProvideSchema schemas,
        CancellationToken ct);

    Task EnrichAsync(Context context,
        CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
