// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Teams.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Teams.Indexes;

public class TeamsIndexTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly ITeamRepository teamRepository = A.Fake<ITeamRepository>();
    private readonly TeamsIndex sut;

    public TeamsIndexTests()
    {
        ct = cts.Token;

        sut = new TeamsIndex(teamRepository);
    }

    [Fact]
    public async Task Should_resolve_teams_by_id()
    {
        var team = SetupTeam(0);

        A.CallTo(() => teamRepository.QueryAllAsync("user1", ct))
            .Returns(new List<ITeamEntity> { team });

        var actual = await sut.GetTeamsAsync("user1", ct);

        Assert.Same(actual[0], team);
    }

    [Fact]
    public async Task Should_return_empty_teams_if_team_not_created()
    {
        var team = SetupTeam(-1);

        A.CallTo(() => teamRepository.QueryAllAsync("user1", ct))
            .Returns(new List<ITeamEntity> { team });

        var actual = await sut.GetTeamsAsync("user1", ct);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_resolve_team_by_id()
    {
        var team = SetupTeam(0);

        A.CallTo(() => teamRepository.FindAsync(team.Id, ct))
            .Returns(team);

        var actual = await sut.GetTeamAsync(team.Id, ct);

        Assert.Same(actual, team);
    }

    [Fact]
    public async Task Should_return_null_team_if_team_not_created()
    {
        var team = SetupTeam(0);

        A.CallTo(() => teamRepository.FindAsync(team.Id, ct))
            .Returns(Task.FromResult<ITeamEntity?>(null));

        var actual = await sut.GetTeamAsync(team.Id, ct);

        Assert.Null(actual);
    }

    private static ITeamEntity SetupTeam(long version)
    {
        var team = Mocks.Team(DomainId.NewGuid());

        A.CallTo(() => team.Version).Returns(version);

        return team;
    }
}
