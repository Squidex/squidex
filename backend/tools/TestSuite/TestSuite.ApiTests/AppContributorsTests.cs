// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary.Management;
using TestSuite.Fixtures;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests
{
    public sealed class AppContributorsTests : IClassFixture<CreatedAppFixture>
    {
        private readonly string email = $"{Guid.NewGuid()}@squidex.io";

        public CreatedAppFixture _ { get; }

        public AppContributorsTests(CreatedAppFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_not_invite_contributor_if_flag_is_false()
        {
            // STEP 0:  Do not invite contributors when flag is false.
            var createRequest = new AssignContributorDto { ContributorId = "test@squidex.io" };

            var ex = await Assert.ThrowsAsync<SquidexManagementException>(() =>
            {
                return _.Apps.PostContributorAsync(_.AppName, createRequest);
            });

            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task Should_invite_contributor()
        {
            // STEP 1: Assign contributor.
            ContributorDto contributor_1 = await InviteAsync();

            Assert.Equal("Developer", contributor_1?.Role);
        }

        [Fact]
        public async Task Should_update_contributor()
        {
            // STEP 0: Assign contributor.
            var contributor = await InviteAsync();


            // STEP 1: Update contributor role.
            var updateRequest = new AssignContributorDto
            {
                ContributorId = email, Role = "Owner"
            };

            var contributors_2 = await _.Apps.PostContributorAsync(_.AppName, updateRequest);
            var contributor_2 = contributors_2.Items.Find(x => x.ContributorId == contributor.ContributorId);

            Assert.Equal(updateRequest.Role, contributor_2?.Role);
        }

        [Fact]
        public async Task Should_remove_contributor()
        {
            // STEP 0: Assign contributor.
            var contributor = await InviteAsync();


            // STEP 1: Remove contributor.
            var contributors_2 = await _.Apps.DeleteContributorAsync(_.AppName, contributor.ContributorId);

            Assert.DoesNotContain(contributors_2.Items, x => x.ContributorId == contributor.ContributorId);
        }

        private async Task<ContributorDto> InviteAsync()
        {
            var createInviteRequest = new AssignContributorDto
            {
                ContributorId = email,
                Invite = true
            };

            var contributors = await _.Apps.PostContributorAsync(_.AppName, createInviteRequest);
            var contributor = contributors.Items.Find(x => x.ContributorName == email);

            return contributor;
        }
    }
}
