// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Teams.Repositories;

public interface ITeamRepository
{
    Task<List<ITeamEntity>> QueryAllAsync(string contributorId,
        CancellationToken ct = default);

    Task<ITeamEntity?> FindAsync(DomainId id,
        CancellationToken ct = default);
}
