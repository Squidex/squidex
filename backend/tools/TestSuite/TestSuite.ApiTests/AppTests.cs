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
    public sealed class AppTests : IClassFixture<CreatedAppFixture>
    {
        public CreatedAppFixture _ { get; }

        public AppTests(CreatedAppFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_get_app()
        {
            // STEP 1: Get app.
            var app = await _.Apps.GetAppAsync(_.AppName);

            Assert.Equal(_.AppName, app.Name);
        }

        [Fact]
        public async Task Should_set_label()
        {
            // STEP 1: Update app
            var updateRequest = new UpdateAppDto
            {
                Label = Guid.NewGuid().ToString()
            };

            var app_1 = await _.Apps.PutAppAsync(_.AppName, updateRequest);

            Assert.Equal(updateRequest.Label, app_1.Label);
        }

        [Fact]
        public async Task Should_set_description()
        {
            // STEP 1: Update app
            var updateRequest = new UpdateAppDto
            {
                Description = Guid.NewGuid().ToString()
            };

            var app_1 = await _.Apps.PutAppAsync(_.AppName, updateRequest);

            Assert.Equal(updateRequest.Description, app_1.Description);
        }

        [Fact]
        public async Task Should_upload_image()
        {
            // STEP 1: Upload image.
            await using (var stream = new FileStream("Assets/logo-squared.png", FileMode.Open))
            {
                var file = new FileParameter(stream, "logo-squared.png", "image/png");

                var app_1 = await _.Apps.UploadImageAsync(_.AppName, file);

                // Should contain image link.
                Assert.True(app_1._links.ContainsKey("image"));
            }


            // STEP 2: Download image.
            await using (var stream = new FileStream("Assets/logo-squared.png", FileMode.Open))
            {
                var temp = new MemoryStream();

                var downloaded = new MemoryStream();

                using (var imageStream = await _.Apps.GetImageAsync(_.AppName))
                {
                    await imageStream.Stream.CopyToAsync(downloaded);
                }

                // Should dowload with correct size.
                Assert.True(downloaded.Length < stream.Length);
            }
        }

        [Fact]
        public async Task Should_delete_image()
        {
            // STEP 1: Upload image.
            await using (var stream = new FileStream("Assets/logo-squared.png", FileMode.Open))
            {
                var file = new FileParameter(stream, "logo-squared.png", "image/png");

                var app_1 = await _.Apps.UploadImageAsync(_.AppName, file);

                // Should contain image link.
                Assert.True(app_1._links.ContainsKey("image"));
            }


            // STEP 2: Delete Image.
            var app_2 = await _.Apps.DeleteImageAsync(_.AppName);

            // Should contain image link.
            Assert.False(app_2._links.ContainsKey("image"));
        }

        [Fact]
        public async Task Should_manage_roles()
        {
            // Use role name with hash to test previous bug.
            var roleName = $"{Guid.NewGuid()}/1";
            var roleClient = Guid.NewGuid().ToString();
            var roleContributor1 = "hello@squidex.io";

            // STEP 1: Add role.
            var createRequest = new AddRoleDto { Name = roleName };

            var roles_1 = await _.Apps.PostRoleAsync(_.AppName, createRequest);
            var role_1 = roles_1.Items.Find(x => x.Name == roleName);

            // Should return role with correct name.
            Assert.Empty(role_1.Permissions);


            // STEP 2: Update role.
            var updateRequest = new UpdateRoleDto { Permissions = new List<string> { "a", "b" } };

            var roles_2 = await _.Apps.PutRoleAsync(_.AppName, roleName, updateRequest);
            var role_2 = roles_2.Items.Find(x => x.Name == roleName);

            // Should return role with correct name.
            Assert.Equal(updateRequest.Permissions, role_2.Permissions);


            // STEP 3: Assign client and contributor.
            await _.Apps.PostClientAsync(_.AppName, new CreateClientDto { Id = roleClient });

            // Add client to role.
            await _.Apps.PutClientAsync(_.AppName, roleClient, new UpdateClientDto { Role = roleName });

            // Add contributor to role.
            await _.Apps.PostContributorAsync(_.AppName, new AssignContributorDto { ContributorId = roleContributor1, Role = roleName, Invite = true });

            var roles_3 = await _.Apps.GetRolesAsync(_.AppName);
            var role_3 = roles_3.Items.Find(x => x.Name == roleName);

            // Should return role with correct number of users and clients.
            Assert.Equal(1, role_3.NumClients);
            Assert.Equal(1, role_3.NumContributors);


            // STEP 4:  Try to delete role.
            var ex = await Assert.ThrowsAnyAsync<SquidexManagementException>(() => _.Apps.DeleteRoleAsync(_.AppName, roleName));

            Assert.Equal(400, ex.StatusCode);


            // STEP 5: Remove after client and contributor removed.
            var fallbackRole = "Developer";

            // Remove client from role.
            await _.Apps.PutClientAsync(_.AppName, roleClient, new UpdateClientDto { Role = fallbackRole });

            // Remove contributor from role.
            await _.Apps.PostContributorAsync(_.AppName, new AssignContributorDto { ContributorId = roleContributor1, Role = fallbackRole });

            await _.Apps.DeleteRoleAsync(_.AppName, roleName);

            var roles_4 = await _.Apps.GetRolesAsync(_.AppName);
            var role_4 = roles_4.Items.Find(x => x.Name == roleName);

            // Should not return deleted role.
            Assert.Null(role_4);
        }

        [Fact]
        public async Task Should_get_settings()
        {
            // STEP 1: Get initial settings.
            var settings_0 = await _.Apps.GetSettingsAsync(_.AppName);

            Assert.NotEmpty(settings_0.Patterns);
        }

        [Fact]
        public async Task Should_update_settings()
        {
            // STEP 1: Update settings with new state.
            var updateRequest = new UpdateAppSettingsDto
            {
                Patterns = new List<PatternDto>
                {
                    new PatternDto { Name = "pattern", Regex = ".*" }
                },
                Editors = new List<EditorDto>
                {
                    new EditorDto { Name = "editor", Url = "http://squidex.io/path/to/editor" }
                }
            };

            var settings_1 = await _.Apps.PutSettingsAsync(_.AppName, updateRequest);

            Assert.NotEmpty(settings_1.Patterns);
            Assert.NotEmpty(settings_1.Editors);
        }
    }
}
