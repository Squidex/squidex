// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Teams;
using Squidex.Domain.Apps.Entities.Teams.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;

namespace Squidex.Domain.Apps.Entities.Teams.Indexes;

public class TeamsIndexTests : GivenContext
{
    private readonly ITeamRepository teamRepository = A.Fake<ITeamRepository>();
    private readonly TeamsIndex sut;

    public TeamsIndexTests()
    {
        sut = new TeamsIndex(teamRepository);
    }

    [Fact]
    public async Task Should_resolve_teams_by_user()
    {
        A.CallTo(() => teamRepository.QueryAllAsync("user1", CancellationToken))
            .Returns([Team]);

        var actual = await sut.GetTeamsAsync("user1", CancellationToken);

        Assert.Same(actual[0], Team);
    }

    [Fact]
    public async Task Should_return_empty_teams_if_team_not_created()
    {
        Team = Team with { Version = -1 };

        A.CallTo(() => teamRepository.QueryAllAsync("user1", CancellationToken))
            .Returns([Team]);

        var actual = await sut.GetTeamsAsync("user1", CancellationToken);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_resolve_team_by_id()
    {
        A.CallTo(() => teamRepository.FindAsync(Team.Id, CancellationToken))
            .Returns(Team);

        var actual = await sut.GetTeamAsync(Team.Id, CancellationToken);

        Assert.Same(actual, Team);
    }

    [Fact]
    public async Task Should_return_null_team_if_team_not_created()
    {
        A.CallTo(() => teamRepository.FindAsync(Team.Id, CancellationToken))
            .Returns(Task.FromResult<Team?>(null));

        var actual = await sut.GetTeamAsync(Team.Id, CancellationToken);

        Assert.Null(actual);
    }

    [Fact]
    public async Task Should_resolve_team_by_domain()
    {
        A.CallTo(() => teamRepository.FindByAuthDomainAsync("squidex.io", CancellationToken))
            .Returns(Team);

        var actual = await sut.GetTeamByAuthDomainAsync("squidex.io", CancellationToken);

        Assert.Same(actual, Team);
    }
}
