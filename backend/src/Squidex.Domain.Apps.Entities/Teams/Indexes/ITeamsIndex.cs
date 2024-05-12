// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Teams;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Teams.Indexes;

public interface ITeamsIndex
{
    Task<Team?> GetTeamAsync(DomainId id,
        CancellationToken ct = default);

    Task<Team?> GetTeamByAuthDomainAsync(string authDomain,
        CancellationToken ct = default);

    Task<List<Team>> GetTeamsAsync(string userId,
        CancellationToken ct = default);
}
