// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes;

public interface ISchemasIndex
{
    Task<Schema?> GetSchemaAsync(DomainId appId, DomainId id, bool canCache,
        CancellationToken ct = default);

    Task<Schema?> GetSchemaAsync(DomainId appId, string name, bool canCache,
        CancellationToken ct = default);

    Task<List<Schema>> GetSchemasAsync(DomainId appId,
        CancellationToken ct = default);
}
