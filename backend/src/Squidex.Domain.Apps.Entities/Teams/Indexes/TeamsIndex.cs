// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Teams.Repositories;
using Squidex.Infrastructure;
using System.Diagnostics;

namespace Squidex.Domain.Apps.Entities.Teams.Indexes;

public sealed class TeamsIndex : ITeamsIndex
{
    private readonly ITeamRepository teamRepository;

    public TeamsIndex(ITeamRepository teamRepository)
    {
        this.teamRepository = teamRepository;
    }

    public async Task<ITeamEntity?> GetTeamAsync(DomainId id,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("TeamsIndex/GetTeamAsync"))
        {
            activity?.SetTag("teamId", id);

            var team = await teamRepository.FindAsync(id, ct);

            return IsValid(team) ? team : null;
        }
    }

    public async Task<List<ITeamEntity>> GetTeamsAsync(string userId,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("TeamsIndex/GetTeamsAsync"))
        {
            activity?.SetTag("userId", userId);

            var teams = await teamRepository.QueryAllAsync(userId, ct);

            return teams.Where(IsValid).ToList();
        }
    }

    private static bool IsValid(ITeamEntity? rule)
    {
        return rule is { Version: > EtagVersion.Empty };
    }
}
