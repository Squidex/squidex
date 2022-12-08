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
public class AppCreationTests : IClassFixture<ClientFixture>
{
    private readonly string appName = Guid.NewGuid().ToString();

    public ClientFixture _ { get; }

    public AppCreationTests(ClientFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_create_app()
    {
        // STEP 1: Create app
        var createRequest = new CreateAppDto
        {
            Name = appName
        };

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

        await Verify(app);
    }

    [Fact]
    public async Task Should_not_allow_creation_if_name_used()
    {
        // STEP 1: Create app
        await CreateAppAsync();


        // STEP 2: Create again and fail
        var createRequest = new CreateAppDto
        {
            Name = appName
        };

        var ex = await Assert.ThrowsAnyAsync<SquidexManagementException>(() =>
        {
            return _.Apps.PostAppAsync(createRequest);
        });

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task Should_archive_app()
    {
        // STEP 1: Create app
        await CreateAppAsync();


        // STEP 2: Archive app
        await _.Apps.DeleteAppAsync(appName);

        var apps = await _.Apps.GetAppsAsync();

        // Should not provide deleted app when apps are queried.
        Assert.DoesNotContain(apps, x => x.Name == appName);
    }

    [Fact]
    public async Task Should_recreate_after_archived()
    {
        // STEP 1: Create app
        await CreateAppAsync();


        // STEP 2: Archive app
        await _.Apps.DeleteAppAsync(appName);


        // STEP 3: Create app again
        var createRequest = new CreateAppDto
        {
            Name = appName
        };

        await _.Apps.PostAppAsync(createRequest);
    }

    [Fact]
    public async Task Should_create_app_from_templates()
    {
        // STEP 1: Get template.
        var templates = await _.Templates.GetTemplatesAsync();

        var template = templates.Items.First(x => x.IsStarter);


        // STEP 2: Create app.
        var createRequest = new CreateAppDto
        {
            Name = appName,
            // The template is just referenced by the name.
            Template = template.Name
        };

        await _.Apps.PostAppAsync(createRequest);


        // STEP 3: Get schemas
        var schemas = await _.Schemas.GetSchemasAsync(appName);

        Assert.NotEmpty(schemas.Items);

        await Verify(schemas);
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
