// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task Should_manage_app_properties()
        {
            var appLabel = Guid.NewGuid().ToString();
            var appDescription = Guid.NewGuid().ToString();

            // STEP 1: Update app
            var updateRequest = new UpdateAppDto { Label = appLabel, Description = appDescription };

            var app_1 = await _.Apps.UpdateAppAsync(_.AppName, updateRequest);

            Assert.Equal(appLabel, app_1.Label);
            Assert.Equal(appDescription, app_1.Description);
        }

        [Fact]
        public async Task Should_manage_image()
        {
            // STEP 1: Upload image.
            using (var stream = new FileStream("Assets/logo-squared.png", FileMode.Open))
            {
                var file = new FileParameter(stream, "logo-squared.png", "image/png");

                var app_1 = await _.Apps.UploadImageAsync(_.AppName, file);

                // Should contain image link.
                Assert.True(app_1._links.ContainsKey("image"));
            }


            // STEP 2: Download image.
            using (var stream = new FileStream("Assets/logo-squared.png", FileMode.Open))
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


            // STEP 3: Delete Image.
            var app_2 = await _.Apps.DeleteImageAsync(_.AppName);

            // Should contain image link.
            Assert.False(app_2._links.ContainsKey("image"));
        }

        [Fact]
        public async Task Should_manage_clients()
        {
            var clientId = Guid.NewGuid().ToString();
            var clientName = "My Client";
            var clientRole1 = "Editor";
            var clientRole2 = "Owner";

            // STEP 1: Create client.
            var createRequest = new CreateClientDto { Id = clientId };

            var clients_1 = await _.Apps.PostClientAsync(_.AppName, createRequest);
            var client_1 = clients_1.Items.Single(x => x.Id == clientId);

            // Should return client with correct name and id.
            Assert.Equal(clientRole1, client_1.Role);
            Assert.Equal(clientId, client_1.Name);


            // STEP 2: Update client name.
            var updateNameRequest = new UpdateClientDto { Name = clientName };

            var clients_2 = await _.Apps.PutClientAsync(_.AppName, clientId, updateNameRequest);
            var client_2 = clients_2.Items.Single(x => x.Id == clientId);

            // Should update client name.
            Assert.Equal(clientName, client_2.Name);


            // STEP 3: Update client role.
            var updateRoleRequest = new UpdateClientDto { Role = clientRole2 };

            var clients_3 = await _.Apps.PutClientAsync(_.AppName, clientId, updateRoleRequest);
            var client_3 = clients_3.Items.Single(x => x.Id == clientId);

            // Should update client role.
            Assert.Equal(clientRole2, client_3.Role);


            // STEP 4: Delete client
            var clients_4 = await _.Apps.DeleteClientAsync(_.AppName, clientId);

            // Should not return deleted client.
            Assert.DoesNotContain(clients_4.Items, x => x.Id == clientId);
        }

        [Fact]
        public async Task Should_manage_contributors()
        {
            var contributorEmail = "hello@squidex.io";
            var contributorRole1 = "Developer";
            var contributorRole2 = "Owner";

            // STEP 0:  Do not invite contributors when flag is false.
            var createRequest = new AssignContributorDto { ContributorId = "test@squidex.io" };

            var ex = await Assert.ThrowsAsync<SquidexManagementException>(() =>
            {
                return _.Apps.PostContributorAsync(_.AppName, createRequest);
            });

            Assert.Equal(404, ex.StatusCode);


            // STEP 1: Assign contributor.
            var createInviteRequest = new AssignContributorDto { ContributorId = contributorEmail, Invite = true };

            var contributors_1 = await _.Apps.PostContributorAsync(_.AppName, createInviteRequest);
            var contributor_1 = contributors_1.Items.FirstOrDefault(x => x.ContributorName == contributorEmail);

            // Should return contributor with correct email.
            Assert.Equal(contributorRole1, contributor_1?.Role);


            // STEP 2: Update contributor role.
            var updateRequest = new AssignContributorDto { ContributorId = contributorEmail, Role = contributorRole2 };

            var contributors_2 = await _.Apps.PostContributorAsync(_.AppName, updateRequest);
            var contributor_2 = contributors_2.Items.FirstOrDefault(x => x.ContributorId == contributor_1.ContributorId);

            // Should return contributor with correct role.
            Assert.Equal(contributorRole2, contributor_2?.Role);


            // STEP 3: Remove contributor.
            var contributors3 = await _.Apps.DeleteContributorAsync(_.AppName, contributor_2.ContributorId);

            // Should not return deleted contributor.
            Assert.DoesNotContain(contributors3.Items, x => x.ContributorId == contributor_2.ContributorId);
        }

        [Fact]
        public async Task Should_manage_roles()
        {
            var roleName = Guid.NewGuid().ToString();
            var roleClient = Guid.NewGuid().ToString();
            var roleContributor1 = "role1@squidex.io";
            var roleContributor2 = "role2@squidex.io";

            // STEP 1: Add role.
            var createRequest = new AddRoleDto { Name = roleName };

            var roles_1 = await _.Apps.PostRoleAsync(_.AppName, createRequest);
            var role_1 = roles_1.Items.Single(x => x.Name == roleName);

            // Should return role with correct name.
            Assert.Empty(role_1.Permissions);


            // STEP 2: Update role.
            var updateRequest = new UpdateRoleDto { Permissions = new List<string> { "a", "b" } };

            var roles_2 = await _.Apps.PutRoleAsync(_.AppName, roleName, updateRequest);
            var role_2 = roles_2.Items.Single(x => x.Name == roleName);

            // Should return role with correct name.
            Assert.Equal(updateRequest.Permissions, role_2.Permissions);


            // STEP 3: Assign client and contributor.
            await _.Apps.PostClientAsync(_.AppName, new CreateClientDto { Id = roleClient });

            await _.Apps.PutClientAsync(_.AppName, roleClient, new UpdateClientDto { Role = roleName });

            await _.Apps.PostContributorAsync(_.AppName, new AssignContributorDto { ContributorId = roleContributor1, Role = roleName, Invite = true });
            await _.Apps.PostContributorAsync(_.AppName, new AssignContributorDto { ContributorId = roleContributor2, Role = roleName, Invite = true });

            var roles_3 = await _.Apps.GetRolesAsync(_.AppName);
            var role_3 = roles_3.Items.Single(x => x.Name == roleName);

            // Should return role with correct number of users and clients.
            Assert.Equal(1, role_3.NumClients);
            Assert.Equal(2, role_3.NumContributors);


            // STEP 4:  Try to delete role.
            var ex = await Assert.ThrowsAsync<SquidexManagementException<ErrorDto>>(() =>
            {
                return _.Apps.DeleteRoleAsync(_.AppName, roleName);
            });

            Assert.Equal(400, ex.StatusCode);


            // STEP 5: Remove after client and contributor removed.
            var fallbackRole = "Developer";

            await _.Apps.PutClientAsync(_.AppName, roleClient, new UpdateClientDto { Role = fallbackRole });

            await _.Apps.PostContributorAsync(_.AppName, new AssignContributorDto { ContributorId = roleContributor1, Role = fallbackRole });
            await _.Apps.PostContributorAsync(_.AppName, new AssignContributorDto { ContributorId = roleContributor2, Role = fallbackRole });

            await _.Apps.DeleteRoleAsync(_.AppName, roleName);

            var roles_4 = await _.Apps.GetRolesAsync(_.AppName);

            // Should not return deleted role.
            Assert.DoesNotContain(roles_4.Items, x => x.Name == roleName);
        }

        [Fact]
        public async Task Should_manage_patterns()
        {
            var patternName = Guid.NewGuid().ToString();
            var patternRegex1 = Guid.NewGuid().ToString();
            var patternRegex2 = Guid.NewGuid().ToString();

            // STEP 1: Add pattern.
            var createRequest = new UpdatePatternDto { Name = patternName, Pattern = patternRegex1 };

            var patterns_1 = await _.Apps.PostPatternAsync(_.AppName, createRequest);
            var pattern_1 = patterns_1.Items.Single(x => x.Name == patternName);

            // Should return pattern with correct regex.
            Assert.Equal(patternRegex1, pattern_1.Pattern);


            // STEP 2: Update pattern.
            var updateRequest = new UpdatePatternDto { Name = patternName, Pattern = patternRegex2 };

            var patterns_2 = await _.Apps.PutPatternAsync(_.AppName, pattern_1.Id, updateRequest);
            var pattern_2 = patterns_2.Items.Single(x => x.Name == patternName);

            // Should return pattern with correct regex.
            Assert.Equal(patternRegex2, pattern_2.Pattern);


            // STEP 3: Remove pattern.
            var patterns_3 = await _.Apps.DeletePatternAsync(_.AppName, pattern_2.Id);

            // Should not return deleted pattern.
            Assert.DoesNotContain(patterns_3.Items, x => x.Id == pattern_2.Id);
        }

        [Fact]
        public async Task Should_manage_languages()
        {
            var appName = Guid.NewGuid().ToString();

            // STEP 1: Add app.
            var createRequest = new CreateAppDto { Name = appName };

            await _.Apps.PostAppAsync(createRequest);


            // STEP 2: Add languages.
            await _.Apps.PostLanguageAsync(appName, new AddLanguageDto { Language = "de" });
            await _.Apps.PostLanguageAsync(appName, new AddLanguageDto { Language = "it" });
            await _.Apps.PostLanguageAsync(appName, new AddLanguageDto { Language = "fr" });

            var languages_1 = await _.Apps.GetLanguagesAsync(appName);
            var languageEN_1 = languages_1.Items.First(x => x.Iso2Code == "en");

            Assert.Equal(new string[] { "en", "de", "fr", "it" }, languages_1.Items.Select(x => x.Iso2Code).ToArray());
            Assert.True(languageEN_1.IsMaster);


            // STEP 3: Update German language.
            var updateRequest1 = new UpdateLanguageDto
            {
                Fallback = new string[]
                {
                    "fr",
                    "it"
                },
                IsOptional = true
            };

            var languages_2 = await _.Apps.PutLanguageAsync(appName, "de", updateRequest1);
            var languageDE_2 = languages_2.Items.First(x => x.Iso2Code == "de");

            Assert.Equal(new string[] { "fr", "it" }, languageDE_2.Fallback.ToArray());
            Assert.True(languageDE_2.IsOptional);


            // STEP 4: Update Italian language.
            var updateRequest2 = new UpdateLanguageDto
            {
                Fallback = new string[]
                {
                    "fr",
                    "de"
                }
            };

            var languages_3 = await _.Apps.PutLanguageAsync(appName, "it", updateRequest2);
            var languageDE_3 = languages_3.Items.First(x => x.Iso2Code == "it");

            Assert.Equal(new string[] { "fr", "de" }, languageDE_3.Fallback.ToArray());


            // STEP 5: Change master language.
            var masterRequest = new UpdateLanguageDto { IsMaster = true };

            var languages_4 = await _.Apps.PutLanguageAsync(appName, "it", masterRequest);

            var languageIT_4 = languages_4.Items.First(x => x.Iso2Code == "it");
            var languageEN_4 = languages_4.Items.First(x => x.Iso2Code == "en");

            Assert.True(languageIT_4.IsMaster);
            Assert.False(languageIT_4.IsOptional);
            Assert.False(languageEN_4.IsMaster);
            Assert.Empty(languageIT_4.Fallback);
            Assert.Equal(new string[] { "it", "de", "en", "fr" }, languages_4.Items.Select(x => x.Iso2Code).ToArray());


            // STEP 6: Remove language.
            var languages_5 = await _.Apps.DeleteLanguageAsync(appName, "fr");
            var languageDE_5 = languages_5.Items.First(x => x.Iso2Code == "de");

            Assert.Equal(new string[] { "it" }, languageDE_5.Fallback.ToArray());
            Assert.Equal(new string[] { "it", "de", "en" }, languages_5.Items.Select(x => x.Iso2Code).ToArray());
        }
    }
}
