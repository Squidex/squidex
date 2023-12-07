// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents;

public interface IContentQueryService
{
    IAsyncEnumerable<EnrichedContent> StreamAsync(Context context, string schemaIdOrName, int skip,
        CancellationToken ct = default);

    Task<IResultList<EnrichedContent>> QueryAsync(Context context, Q q,
        CancellationToken ct = default);

    Task<IResultList<EnrichedContent>> QueryAsync(Context context, string schemaIdOrName, Q query,
        CancellationToken ct = default);

    Task<EnrichedContent?> FindAsync(Context context, string schemaIdOrName, DomainId id, long version = EtagVersion.Any,
        CancellationToken ct = default);

    Task<Schema> GetSchemaOrThrowAsync(Context context, string schemaIdOrName,
        CancellationToken ct = default);

    Task<Schema?> GetSchemaAsync(Context context, string schemaIdOrNama,
        CancellationToken ct = default);
}
