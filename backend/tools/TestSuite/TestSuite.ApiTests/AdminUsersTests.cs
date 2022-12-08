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

public sealed class AdminUsersTests : IClassFixture<ClientFixture>
{
    private readonly string email = $"{Guid.NewGuid()}@squidex.io";

    public ClientFixture _ { get; }

    public AdminUsersTests(ClientFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_create_user()
    {
        // STEP 1: Create user.
        var user_0 = await CreateUserAsync();

        Assert.Equal(email, user_0.Email);
        Assert.Equal(email, user_0.DisplayName);


        // STEP 2: Get user by ID.
        var userById = await _.UserManagement.GetUserAsync(user_0.Id);

        Assert.Equal(email, userById.Email);
        Assert.Equal(email, userById.DisplayName);


        // STEP 2: Get users by email.
        var usersByEmail = await _.UserManagement.GetUsersAsync(user_0.Email);

        Assert.Equal(email, usersByEmail.Items.First().Email);
        Assert.Equal(email, usersByEmail.Items.First().DisplayName);
    }

    [Fact]
    public async Task Should_update_user()
    {
        // STEP 0: Create user.
        var user_0 = await CreateUserAsync();


        // STEP 2: Update user.
        var updateRequest = new UpdateUserDto
        {
            DisplayName = Guid.NewGuid().ToString(),
            // The API requests to also set the email address.
            Email = email,
        };

        var user_1 = await _.UserManagement.PutUserAsync(user_0.Id, updateRequest);

        Assert.Equal(updateRequest.DisplayName, user_1.DisplayName);
    }

    [Fact]
    public async Task Should_lock_user()
    {
        // STEP 0: Create user.
        var user_0 = await CreateUserAsync();


        // STEP 1: Lock user.
        var user_1 = await _.UserManagement.LockUserAsync(user_0.Id);

        Assert.True(user_1.IsLocked);


        // STEP 2: Unlock user.
        var user_2 = await _.UserManagement.UnlockUserAsync(user_0.Id);

        Assert.False(user_2.IsLocked);
    }

    [Fact]
    public async Task Should_delete_user()
    {
        // STEP 0: Create user.
        var user_0 = await CreateUserAsync();

        Assert.Equal(email, user_0.Email);
        Assert.Equal(email, user_0.DisplayName);


        // STEP 1: Delete user
        await _.UserManagement.DeleteUserAsync(user_0.Id);


        // STEP 2: Get user by email.
        var usersByEmail = await _.UserManagement.GetUsersAsync(user_0.Email);

        Assert.Empty(usersByEmail.Items);
    }

    private async Task<UserDto> CreateUserAsync()
    {
        var createRequest = new CreateUserDto
        {
            Email = email,
            Password = "1q2w3e$R",
            Permissions = new List<string>(),
            // The API requests to also set the display name.
            DisplayName = email,
        };

        return await _.UserManagement.PostUserAsync(createRequest);
    }
}
