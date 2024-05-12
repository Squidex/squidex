// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FluentAssertions;
using Squidex.ClientLibrary;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class TeamTests : IClassFixture<CreatedTeamFixture>
{
    public CreatedTeamFixture _ { get; }

    public TeamTests(CreatedTeamFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_set_name()
    {
        // STEP 1: Update app.
        var updateRequest = new UpdateTeamDto
        {
            Name = Guid.NewGuid().ToString()
        };

        var app_1 = await _.Client.Teams.PutTeamAsync(_.TeamId, updateRequest);

        Assert.Equal(updateRequest.Name, app_1.Name);
    }

    [Fact]
    public async Task Should_transfer_app_to_team()
    {
        // STEP 0: Create app.
        var (client, _) = await _.PostAppAsync();


        // STEP 1: Assign app to team.
        var transferRequest = new TransferToTeamDto
        {
            TeamId = _.TeamId
        };

        var app_1 = await client.Apps.PutAppTeamAsync(transferRequest);

        Assert.Equal(_.TeamId, app_1.TeamId);
    }

    [Fact]
    public async Task Should_remove_app_from_team()
    {
        // STEP 0: Create app.
        var (client, _) = await _.PostAppAsync();


        // STEP 1: Assign app to team.
        var transferRequest = new TransferToTeamDto
        {
            TeamId = _.TeamId
        };

        var app_1 = await client.Apps.PutAppTeamAsync(transferRequest);

        Assert.Equal(_.TeamId, app_1.TeamId);


        // STEP 2: Remove app from team.
        var untransferRequest = new TransferToTeamDto
        {
            TeamId = null
        };

        var app_2 = await client.Apps.PutAppTeamAsync(untransferRequest);

        Assert.Null(app_2.TeamId);
    }

    [Fact]
    public async Task Should_assign_scheme()
    {
        var scheme = new AuthSchemeDto
        {
            Authority = "https://squidex.io",
            Domain = "squidex.io",
            DisplayName = "Squidex",
            ClientId = "ID",
            ClientSecret = "secret",
        };

        // STEP 1: Assign scheme.
        var request = new AuthSchemeValueDto
        {
            Scheme = scheme
        };

        var scheme_0 = await _.Client.Teams.PutTeamAuthAsync(_.TeamId, request);

        scheme_0.Scheme.Should().BeEquivalentTo(scheme);


        // STEP 2: Get scheme.
        var scheme_1 = await _.Client.Teams.GetTeamAuthAsync(_.TeamId);

        scheme_1.Scheme.Should().BeEquivalentTo(scheme);
    }

    [Fact]
    public async Task Should_unassign_scheme()
    {
        var scheme = new AuthSchemeDto
        {
            Authority = "https://squidex.io",
            Domain = "squidex.io",
            DisplayName = "Squidex",
            ClientId = "ID",
            ClientSecret = "secret",
        };

        // STEP 0: Assign scheme.
        var request1 = new AuthSchemeValueDto
        {
            Scheme = scheme
        };

        await _.Client.Teams.PutTeamAuthAsync(_.TeamId, request1);


        // STEP 1: Unassign scheme.
        var request2 = new AuthSchemeValueDto
        {
            Scheme = null
        };

        var scheme_0 = await _.Client.Teams.PutTeamAuthAsync(_.TeamId, request2);

        Assert.Null(scheme_0.Scheme);


        // STEP 1: Get scheme.
        var scheme_1 = await _.Client.Teams.GetTeamAuthAsync(_.TeamId);

        Assert.Null(scheme_1.Scheme);
    }
}
