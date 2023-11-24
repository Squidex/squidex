// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace TestSuite.ApiTests;

public class TeamCreationTests : IClassFixture<ClientFixture>
{
    private readonly string teamName = Guid.NewGuid().ToString();

    public ClientFixture _ { get; }

    public TeamCreationTests(ClientFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_create_team()
    {
        var request = new CreateTeamDto
        {
            Name = teamName
        };

        var team = await _.Client.Teams.PostTeamAsync(request);

        Assert.Equal(teamName, team.Name);
    }

    [Fact]
    public async Task Should_create_team_with_duplicate_name()
    {
        var request = new CreateTeamDto
        {
            Name = teamName
        };

        var team1 = await _.Client.Teams.PostTeamAsync(request);
        var team2 = await _.Client.Teams.PostTeamAsync(request);

        Assert.Equal(teamName, team1.Name);
        Assert.Equal(teamName, team2.Name);
        Assert.NotEqual(team1.Id, team2.Id);
    }
}
