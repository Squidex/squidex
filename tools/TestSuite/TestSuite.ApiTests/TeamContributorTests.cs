// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class TeamContributorTests : IClassFixture<ClientFixture>
{
    public ClientFixture _ { get; }

    public TeamContributorTests(ClientFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_not_invite_contributor_if_flag_is_false()
    {
        // STEP 0: Create team.
        var team = await _.PostTeamAsync();


        // STEP 1:  Do not invite contributors when flag is false.
        var createRequest = new AssignContributorDto
        {
            ContributorId = "test@squidex.io"
        };

        var ex = await Assert.ThrowsAnyAsync<SquidexException>(() =>
        {
            return _.Client.Teams.PostContributorAsync(team.Id, createRequest);
        });

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task Should_invite_contributor()
    {
        // STEP 0: Create team.
        var team = await _.PostTeamAsync();


        // STEP 0: Create app.
        var (app, _) = await _.PostAppAsync();


        // STEP 1: Assign contributor.
        ContributorDto contributor_1 = await InviteAsync(team.Id);

        Assert.Equal("Owner", contributor_1?.Role);

        await Verify(contributor_1)
            .IgnoreMember<ContributorDto>(x => x.ContributorId)
            .IgnoreMember<ContributorDto>(x => x.ContributorEmail)
            .IgnoreMember<ContributorDto>(x => x.ContributorName);
    }

    [Fact]
    public async Task Should_remove_contributor()
    {
        // STEP 0: Create team.
        var team = await _.PostTeamAsync();


        // STEP 1: Assign first contributor.
        await InviteAsync(team.Id);


        // STEP 2: Assign other contributor.
        var contributor2 = await InviteAsync(team.Id);


        // STEP 2: Remove contributor.
        var contributors_2 = await _.Client.Teams.DeleteContributorAsync(team.Id, contributor2.ContributorId);

        Assert.DoesNotContain(contributors_2.Items, x => x.ContributorId == contributor2.ContributorId);

        await Verify(contributors_2)
            .IgnoreMember<ContributorDto>(x => x.ContributorId)
            .IgnoreMember<ContributorDto>(x => x.ContributorEmail)
            .IgnoreMember<ContributorDto>(x => x.ContributorName);
    }

    [Fact]
    public async Task Should_not_remove_single_owner()
    {
        // STEP 0: Create team.
        var team = await _.PostTeamAsync();


        // STEP 1: Get contributors
        var contributor = await InviteAsync(team.Id);


        // STEP 2: Remove contributor.
        await Assert.ThrowsAnyAsync<SquidexException>(() => _.Client.Teams.DeleteContributorAsync(team.Id, contributor.ContributorId));
    }

    private async Task<ContributorDto> InviteAsync(string teamId)
    {
        var email = $"{Guid.NewGuid()}@squidex.io";

        var createInviteRequest = new AssignContributorDto
        {
            ContributorId = email,
            // Invite must be true, otherwise new users are not created.
            Invite = true,
            // This is the only allowed role for teams.
            Role = "Owner"
        };

        var contributors = await _.Client.Teams.PostContributorAsync(teamId, createInviteRequest);
        var contributor = contributors.Items.Find(x => x.ContributorName == email);

        return contributor!;
    }
}
