// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using ApiTest.Fixtures;
using Squidex.ClientLibrary.Management;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace ApiTest
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

            var createRequest = new CreateAppDto { Name = appName };

            var app = await _.Apps.PostAppAsync(createRequest);

            // Should return create app with correct name.
            Assert.Equal(appName, app.Name);

            var apps = await _.Apps.GetAppsAsync();

            // Should provide new app when apps are queried.
            Assert.Contains(apps, x => x.Name == appName);

            var contributors = await _.Apps.GetContributorsAsync(appName);

            // Should not client itself as a contributor.
            Assert.Empty(contributors.Items);

            var clients = await _.Apps.GetClientsAsync(appName);

            // Should create default client.
            Assert.Contains(clients.Items, x => x.Id == "default");
        }

        [Fact]
        public async Task Should_remove_app()
        {
            var appName = Guid.NewGuid().ToString();

            await _.Apps.PostAppAsync(new CreateAppDto { Name = appName });
            await _.Apps.DeleteAppAsync(appName);

            var apps = await _.Apps.GetAppsAsync();

            // Should not provide deleted app when apps are queried.
            Assert.DoesNotContain(apps, x => x.Name == appName);
        }
    }
}
