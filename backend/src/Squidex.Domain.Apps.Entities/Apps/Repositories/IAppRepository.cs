// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.Repositories;

public interface IAppRepository
{
    Task<List<IAppEntity>> QueryAllAsync(string contributorId, IEnumerable<string> names,
        CancellationToken ct = default);

    Task<List<IAppEntity>> QueryAllAsync(DomainId teamId,
        CancellationToken ct = default);

    Task<IAppEntity?> FindAsync(DomainId id,
        CancellationToken ct = default);

    Task<IAppEntity?> FindAsync(string name,
        CancellationToken ct = default);
}
