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

public sealed class AppRolesTests : IClassFixture<CreatedAppFixture>
{
    private readonly string roleName = Guid.NewGuid().ToString();
    private readonly string client = Guid.NewGuid().ToString();
    private readonly string contributor = $"{Guid.NewGuid()}@squidex.io";

    public CreatedAppFixture _ { get; }

    public AppRolesTests(CreatedAppFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_create_role()
    {
        // STEP 1: Add role..
        var role = await CreateRoleAsync(roleName);

        // Should return role with correct name.
        Assert.Empty(role.Permissions);

        await Verify(role)
            .IgnoreMember<RoleDto>(x => x.Name);
    }

    [Fact]
    public async Task Should_create_role_with_buggy_name()
    {
        // STEP 1: Add role..
        var role = await CreateRoleAsync($"{Guid.NewGuid()}/1");

        // Should return role with correct name.
        Assert.Empty(role.Permissions);

        await Verify(role)
            .IgnoreMember<RoleDto>(x => x.Name);
    }

    [Fact]
    public async Task Should_update_role()
    {
        // STEP 1: Add role..
        var role = await CreateRoleAsync(roleName);


        // STEP 2: Update role..
        var updateRequest = new UpdateRoleDto
        {
            Permissions = ["a", "b"]
        };

        var roles_2 = await _.Client.Apps.PutRoleAsync(roleName, updateRequest);
        var role_2 = roles_2.Items.Find(x => x.Name == roleName);

        // Should return role with correct name.
        Assert.Equal(updateRequest.Permissions, role_2?.Permissions);

        await Verify(role_2)
            .IgnoreMember<RoleDto>(x => x.Name);
    }

    [Fact]
    public async Task Should_prevent_deletion_if_client_assigned()
    {
        // STEP 1: Add role..
        var role = await CreateRoleAsync(roleName);


        // STEP 2 Assign client and contributor.
        var createClientRequest = new CreateClientDto
        {
            Id = client
        };

        await _.Client.Apps.PostClientAsync(createClientRequest);

        await AssignClient(roleName);

        var roles_2 = await _.Client.Apps.GetRolesAsync();
        var role_2 = roles_2.Items.Find(x => x.Name == roleName);

        // Should return role with correct number of users and clients.
        Assert.Equal(1, role_2?.NumClients);
        Assert.Equal(0, role_2?.NumContributors);


        // STEP 4: Try to delete role.
        var ex = await Assert.ThrowsAnyAsync<SquidexException>(() =>
        {
            return _.Client.Apps.DeleteRoleAsync(roleName);
        });

        Assert.Equal(400, ex.StatusCode);


        // STEP 5: AssignClient client.
        await AssignClient("Developer");

        var roles_3 = await _.Client.Apps.DeleteRoleAsync(roleName);

        Assert.DoesNotContain(roles_3.Items, x => x.Name == roleName);
    }

    [Fact]
    public async Task Should_prevent_deletion_if_contributor_assigned()
    {
        // STEP 1: Add role.
        var role = await CreateRoleAsync(roleName);


        // STEP 2 Assign contributor..
        await AssignContributor(roleName);

        var roles_2 = await _.Client.Apps.GetRolesAsync();
        var role_2 = roles_2.Items.Find(x => x.Name == roleName);

        // Should return role with correct number of users and clients.
        Assert.Equal(0, role_2?.NumClients);
        Assert.Equal(1, role_2?.NumContributors);


        // STEP 4: Try to delete role..
        var ex = await Assert.ThrowsAnyAsync<SquidexException>(() =>
        {
            return _.Client.Apps.DeleteRoleAsync(roleName);
        });

        Assert.Equal(400, ex.StatusCode);


        // STEP 5: Remove role after contributor removed..
        await AssignContributor("Developer");

        var roles_3 = await _.Client.Apps.DeleteRoleAsync(roleName);

        Assert.DoesNotContain(roles_3.Items, x => x.Name == roleName);
    }

    private async Task AssignContributor(string? role = null)
    {
        var assignRequest = new AssignContributorDto
        {
            ContributorId = contributor,
            // Test diffferent role names.
            Role = role,
            // Invite must be true, otherwise new users are not created.
            Invite = true
        };

        await _.Client.Apps.PostContributorAsync(assignRequest);
    }

    private async Task AssignClient(string? role = null)
    {
        var updateRequest = new UpdateClientDto
        {
            Role = role
        };

        await _.Client.Apps.PutClientAsync(client, updateRequest);
    }

    private async Task<RoleDto> CreateRoleAsync(string name)
    {
        var createRequest = new AddRoleDto
        {
            Name = name
        };

        var roles = await _.Client.Apps.PostRoleAsync(createRequest);
        var role = roles.Items.Find(x => x.Name == name);

        return role!;
    }
}
