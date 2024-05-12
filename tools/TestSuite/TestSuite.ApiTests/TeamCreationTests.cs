// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

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
        var team = await _.PostTeamAsync(teamName);

        Assert.Equal(teamName, team.Name);
    }

    [Fact]
    public async Task Should_create_team_with_duplicate_name()
    {
        var team1 = await _.PostTeamAsync(teamName);
        var team2 = await _.PostTeamAsync(teamName);

        Assert.Equal(teamName, team1.Name);
        Assert.Equal(teamName, team2.Name);
        Assert.NotEqual(team1.Id, team2.Id);
    }

    [Fact]
    public async Task Should_get_team_by_id()
    {
        // STEP 0: Create team.
        var team_0 = await _.PostTeamAsync(teamName);


        // STEP 2: Get team.
        var team_1 = await _.Client.Teams.GetTeamAsync(team_0.Id);

        Assert.Equal(team_0.Id, team_1.Id);
    }

    [Fact]
    public async Task Should_archive_team()
    {
        // STEP 1: Create team.
        var team = await _.PostTeamAsync(teamName);


        // STEP 2: Archive app.
        await _.Client.Teams.DeleteTeamAsync(team.Id);

        var teams = await _.Client.Teams.GetTeamsAsync();

        // Should not provide deleted team when teams are queried.
        Assert.DoesNotContain(teams, x => x.Id == team.Id);
    }
}
