// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Teams;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Teams.Repositories;

public interface ITeamRepository
{
    Task<List<Team>> QueryAllAsync(string contributorId,
        CancellationToken ct = default);

    Task<Team?> FindAsync(DomainId id,
        CancellationToken ct = default);

    Task<Team?> FindByAuthDomainAsync(string authDomain,
        CancellationToken ct = default);
}
