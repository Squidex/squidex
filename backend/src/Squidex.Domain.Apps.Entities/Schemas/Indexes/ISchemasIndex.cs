// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Schemas.Indexes;

public interface ISchemasIndex
{
    Task<ISchemaEntity?> GetSchemaAsync(DomainId appId, DomainId id, bool canCache,
        CancellationToken ct = default);

    Task<ISchemaEntity?> GetSchemaAsync(DomainId appId, string name, bool canCache,
        CancellationToken ct = default);

    Task<List<ISchemaEntity>> GetSchemasAsync(DomainId appId,
        CancellationToken ct = default);
}
