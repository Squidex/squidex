// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.ClientLibrary.Management;
using TestSuite.Fixtures;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests
{
    public class AppCreationTests : IClassFixture<ClientFixture>
    {
        public ClientFixture _ { get; }

        public AppCreationTests(ClientFixture fixture)
        {
            _ = fixture;
        }

        [Fact]
        public async Task Should_create_app()
        {
            var appName = Guid.NewGuid().ToString();

            // STEP 1: Create app
            var createRequest = new CreateAppDto { Name = appName };

            var app = await _.Apps.PostAppAsync(createRequest);

            // Should return created app with correct name.
            Assert.Equal(appName, app.Name);


            // STEP 2: Get all apps
            var apps = await _.Apps.GetAppsAsync();

            // Should provide new app when apps are queried.
            Assert.Contains(apps, x => x.Name == appName);


            // STEP 3: Check contributors
            var contributors = await _.Apps.GetContributorsAsync(appName);

            // Should not add client itself as a contributor.
            Assert.Empty(contributors.Items);


            // STEP 4: Check clients
            var clients = await _.Apps.GetClientsAsync(appName);

            // Should create default client.
            Assert.Contains(clients.Items, x => x.Id == "default");
        }

        [Fact]
        public async Task Should_archive_app()
        {
            var appName = Guid.NewGuid().ToString();

            // STEP 1: Create app
            var createRequest = new CreateAppDto { Name = appName };

            await _.Apps.PostAppAsync(createRequest);


            // STEP 2: Archive app
            await _.Apps.DeleteAppAsync(appName);

            var apps = await _.Apps.GetAppsAsync();

            // Should not provide deleted app when apps are queried.
            Assert.DoesNotContain(apps, x => x.Name == appName);
        }

        [Fact]
        public async Task Should_recreate_after_archived()
        {
            var appName = Guid.NewGuid().ToString();

            // STEP 1: Create app
            var createRequest = new CreateAppDto { Name = appName };

            await _.Apps.PostAppAsync(createRequest);


            // STEP 2: Archive app
            await _.Apps.DeleteAppAsync(appName);


            // STEP 3: Create app again
            await _.Apps.PostAppAsync(createRequest);
        }
    }
}
