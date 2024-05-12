// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Teams;
using Squidex.Domain.Apps.Entities.Teams.Repositories;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Teams.Indexes;

public sealed class TeamsIndex : ITeamsIndex
{
    private readonly ITeamRepository teamRepository;

    public TeamsIndex(ITeamRepository teamRepository)
    {
        this.teamRepository = teamRepository;
    }

    public async Task<Team?> GetTeamAsync(DomainId id,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("TeamsIndex/GetTeamAsync"))
        {
            activity?.SetTag("teamId", id);

            var team = await teamRepository.FindAsync(id, ct);

            return IsValid(team) ? team : null;
        }
    }

    public async Task<Team?> GetTeamByAuthDomainAsync(string authDomain,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("TeamsIndex/GetTeamByAuthDomainAsync"))
        {
            activity?.SetTag("authDomain", authDomain);

            var team = await teamRepository.FindByAuthDomainAsync(authDomain, ct);

            return IsValid(team) ? team : null;
        }
    }

    public async Task<List<Team>> GetTeamsAsync(string userId,
        CancellationToken ct = default)
    {
        using (var activity = Telemetry.Activities.StartActivity("TeamsIndex/GetTeamsAsync"))
        {
            activity?.SetTag("userId", userId);

            var teams = await teamRepository.QueryAllAsync(userId, ct);

            return teams.Where(IsValid).ToList();
        }
    }

    private static bool IsValid(Team? team)
    {
        return team is { Version: > EtagVersion.Empty, IsDeleted: false };
    }
}
