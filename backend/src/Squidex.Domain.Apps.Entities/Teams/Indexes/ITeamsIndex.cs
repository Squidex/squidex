// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Teams.Indexes;

public interface ITeamsIndex
{
    Task<ITeamEntity?> GetTeamAsync(DomainId id,
        CancellationToken ct = default);

    Task<List<ITeamEntity>> GetTeamsAsync(string userId,
        CancellationToken ct = default);
}
