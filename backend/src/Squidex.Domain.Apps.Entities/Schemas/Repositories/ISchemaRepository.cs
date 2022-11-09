// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Schemas.Repositories;

public interface ISchemaRepository
{
    Task<List<ISchemaEntity>> QueryAllAsync(DomainId appId,
        CancellationToken ct = default);

    Task<ISchemaEntity?> FindAsync(DomainId appId, DomainId id,
        CancellationToken ct = default);

    Task<ISchemaEntity?> FindAsync(DomainId appId, string name,
        CancellationToken ct = default);
}
