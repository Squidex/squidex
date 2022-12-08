// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary.Management;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

[UsesVerify]
public sealed class AppContributorsTests : IClassFixture<ClientFixture>
{
    private readonly string appName = Guid.NewGuid().ToString();
    private readonly string email = $"{Guid.NewGuid()}@squidex.io";

    public ClientFixture _ { get; }

    public AppContributorsTests(ClientFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_not_invite_contributor_if_flag_is_false()
    {
        // STEP 0: Create app.
        await CreateAppAsync();


        // STEP 1:  Do not invite contributors when flag is false.
        var createRequest = new AssignContributorDto
        {
            ContributorId = "test@squidex.io"
        };

        var ex = await Assert.ThrowsAnyAsync<SquidexManagementException>(() =>
        {
            return _.Apps.PostContributorAsync(appName, createRequest);
        });

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task Should_invite_contributor()
    {
        // STEP 0: Create app.
        await CreateAppAsync();


        // STEP 1: Assign contributor.
        ContributorDto contributor_1 = await InviteAsync();

        Assert.Equal("Developer", contributor_1?.Role);

        await Verify(contributor_1)
            .IgnoreMember<ContributorDto>(x => x.ContributorId)
            .IgnoreMember<ContributorDto>(x => x.ContributorEmail)
            .IgnoreMember<ContributorDto>(x => x.ContributorName);
    }

    [Fact]
    public async Task Should_update_contributor()
    {
        // STEP 0: Create app.
        await CreateAppAsync();


        // STEP 1: Assign contributor.
        var contributor = await InviteAsync();


        // STEP 1: Update contributor role.
        var updateRequest = new AssignContributorDto
        {
            ContributorId = email,
            // Test update of role.
            Role = "Owner"
        };

        var contributors_2 = await _.Apps.PostContributorAsync(appName, updateRequest);
        var contributor_2 = contributors_2.Items.Find(x => x.ContributorId == contributor.ContributorId);

        Assert.Equal(updateRequest.Role, contributor_2?.Role);
    }

    [Fact]
    public async Task Should_remove_contributor()
    {
        // STEP 0: Create app.
        await CreateAppAsync();


        // STEP 1: Assign contributor.
        var contributor = await InviteAsync();


        // STEP 1: Remove contributor.
        var contributors_2 = await _.Apps.DeleteContributorAsync(appName, contributor.ContributorId);

        Assert.DoesNotContain(contributors_2.Items, x => x.ContributorId == contributor.ContributorId);

        await Verify(contributors_2);
    }

    private async Task<ContributorDto> InviteAsync()
    {
        var createInviteRequest = new AssignContributorDto
        {
            ContributorId = email,
            // Invite must be true, otherwise new users are not created.
            Invite = true
        };

        var contributors = await _.Apps.PostContributorAsync(appName, createInviteRequest);
        var contributor = contributors.Items.Find(x => x.ContributorName == email);

        return contributor;
    }

    private async Task CreateAppAsync()
    {
        var createRequest = new CreateAppDto
        {
            Name = appName
        };

        await _.Apps.PostAppAsync(createRequest);
    }
}
