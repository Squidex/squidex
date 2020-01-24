// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiTest.Fixtures;
using Squidex.ClientLibrary.Management;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace ApiTest
{
    public sealed class AppsTests : IClassFixture<CreatedAppFixture>
    {
        public CreatedAppFixture _ { get; }

        public AppsTests(CreatedAppFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_manage_clients()
        {
            var clientId = "my-client";
            var clientName = "My Client";
            var clientRole = "Owner";

            // STEP 1: Create client
            var createRequest = new CreateClientDto { Id = clientId };

            var clients1 = await _.Apps.PostClientAsync(_.AppName, createRequest);

            // Should return client with correct name and id.
            Assert.Contains(clients1.Items, x => x.Id == clientId && x.Name == clientId && x.Role == "Editor");


            // STEP 2: Update client name.
            var updateNameRequest = new UpdateClientDto { Name = clientName };

            var clients2 = await _.Apps.PutClientAsync(_.AppName, clientId, updateNameRequest);

            // Should update client name.
            Assert.Contains(clients2.Items, x => x.Id == clientId && x.Name == clientName && x.Role == "Editor");


            // STEP 3: Update client role.
            var updateRoleRequest = new UpdateClientDto { Role = clientRole };

            var clients3 = await _.Apps.PutClientAsync(_.AppName, clientId, updateRoleRequest);

            // Should update client role.
            Assert.Contains(clients3.Items, x => x.Id == clientId && x.Name == clientName && x.Role == clientRole);


            // STEP 4: Delete client
            var clients4 = await _.Apps.DeleteClientAsync(_.AppName, clientId);

            // Should not return deleted client.
            Assert.DoesNotContain(clients4.Items, x => x.Id == clientId);
        }

        [Fact]
        public async Task Should_manage_contributors()
        {
            var contributorEmail = "hello@squidex.io";
            var contributorRole = "Owner";

            // STEP 0:  Do not invite contributors when flag is false.
            var createRequest = new AssignContributorDto { ContributorId = "test@squidex.io" };

            var ex = await Assert.ThrowsAsync<SquidexManagementException>(() =>
            {
                return _.Apps.PostContributorAsync(_.AppName, createRequest);
            });

            Assert.Equal(404, ex.StatusCode);


            // STEP 1: Assign contributor.
            var createInviteRequest = new AssignContributorDto { ContributorId = contributorEmail, Invite = true };

            var contributors1 = await _.Apps.PostContributorAsync(_.AppName, createInviteRequest);

            var id = contributors1.Items.FirstOrDefault(x => x.ContributorName == contributorEmail).ContributorId;

            // Should return contributor with correct email.
            Assert.Contains(contributors1.Items, x => x.ContributorName == contributorEmail && x.Role == "Developer");


            // STEP 2: Update contributor role.
            var updateRequest = new AssignContributorDto { ContributorId = contributorEmail, Role = contributorRole };

            var contributors2 = await _.Apps.PostContributorAsync(_.AppName, updateRequest);

            // Should return contributor with correct role.
            Assert.Contains(contributors2.Items, x => x.ContributorId == id && x.Role == contributorRole);


            // STEP 3: Remove contributor.
            var contributors3 = await _.Apps.DeleteContributorAsync(_.AppName, id);

            // Should not return deleted contributor.
            Assert.DoesNotContain(contributors3.Items, x => x.ContributorId == id);
        }

        [Fact]
        public async Task Should_manage_roles()
        {
            var roleName = Guid.NewGuid().ToString();
            var roleClient = Guid.NewGuid().ToString();
            var roleContributor = "role@squidex.io";

            // STEP 1: Add role.
            var createRequest = new AddRoleDto { Name = roleName };

            var roles1 = await _.Apps.PostRoleAsync(_.AppName, createRequest);

            // Should return role with correct name.
            Assert.Contains(roles1.Items, x => x.Name == roleName && x.Permissions.Count == 0);


            // STEP 2: Update role.
            var updateRequest = new UpdateRoleDto { Permissions = new List<string> { "a", "b" } };

            var roles2 = await _.Apps.PutRoleAsync(_.AppName, roleName, updateRequest);

            // Should return role with correct name.
            Assert.Contains(roles2.Items, x => x.Name == roleName && x.Permissions.SequenceEqual(updateRequest.Permissions));


            // STEP 3: Assign client and contributor.
            await _.Apps.PostClientAsync(_.AppName, new CreateClientDto { Id = roleClient });
            await _.Apps.PutClientAsync(_.AppName, roleClient, new UpdateClientDto { Role = roleName });

            await _.Apps.PostContributorAsync(_.AppName, new AssignContributorDto { ContributorId = roleContributor, Role = roleName, Invite = true });

            var roles3 = await _.Apps.GetRolesAsync(_.AppName);

            // Should return role with correct number of users and clients.
            Assert.Contains(roles3.Items, x => x.Name == roleName && x.NumClients == 1 && x.NumContributors == 1);


            // STEP 4:  Try to delete role.
            var ex = await Assert.ThrowsAsync<SquidexManagementException<ErrorDto>>(() =>
            {
                return _.Apps.DeleteRoleAsync(_.AppName, roleName);
            });

            Assert.Equal(400, ex.StatusCode);


            // Step 5: Remove after client and contributor removed.
            var fallbackRole = "Developer";

            await _.Apps.PutClientAsync(_.AppName, roleClient, new UpdateClientDto { Role = fallbackRole });
            await _.Apps.PostContributorAsync(_.AppName, new AssignContributorDto { ContributorId = roleContributor, Role = fallbackRole });

            await _.Apps.DeleteRoleAsync(_.AppName, roleName);

            var roles4 = await _.Apps.GetRolesAsync(_.AppName);

            // Should not return deleted role.
            Assert.DoesNotContain(roles4.Items, x => x.Name == roleName);
        }
    }
}
