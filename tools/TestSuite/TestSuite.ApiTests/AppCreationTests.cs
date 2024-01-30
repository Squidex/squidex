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
        // STEP 1: Create app.
        var (app, dto) = await _.PostAppAsync(appName);

        // Should return created app with correct name.
        Assert.Equal(appName, app.Options.AppName);


        // STEP 2: Get all apps.
        var apps = await app.Apps.GetAppsAsync();

        // Should provide new app when apps are queried.
        Assert.Contains(apps, x => x.Name == appName);


        // STEP 3: Check contributors.
        var contributors = await app.Apps.GetContributorsAsync();

        // Should not add client itself as a contributor.
        Assert.Empty(contributors.Items);


        // STEP 4: Check clients.
        var clients = await app.Apps.GetClientsAsync();

        // Should create default client.
        Assert.Contains(clients.Items, x => x.Id == "default");

        await Verify(dto);
    }

    [Fact]
    public async Task Should_not_allow_creation_if_name_used()
    {
        // STEP 1: Create app.
        await _.PostAppAsync(appName);


        // STEP 2: Create again and fail.
        var ex = await Assert.ThrowsAnyAsync<SquidexException>(() =>
        {
            return _.PostAppAsync(appName);
        });

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task Should_archive_app()
    {
        // STEP 1: Create app.
        var (app, _) = await _.PostAppAsync(appName);


        // STEP 2: Archive app.
        await app.Apps.DeleteAppAsync();

        var apps = await app.Apps.GetAppsAsync();

        // Should not provide deleted app when apps are queried.
        Assert.DoesNotContain(apps, x => x.Name == appName);
    }

    [Fact]
    public async Task Should_recreate_after_archived()
    {
        // STEP 1: Create app.
        var (app, _) = await _.PostAppAsync(appName);


        // STEP 2: Archive app.
        await app.Apps.DeleteAppAsync();


        // STEP 3: Create app again.
        await _.PostAppAsync(appName);
    }

    [Fact]
    public async Task Should_create_app_from_templates()
    {
        // STEP 1: Get template.
        var templates = await _.Client.Templates.GetTemplatesAsync();

        var template = templates.Items.First(x => x.IsStarter);


        // STEP 2: Create app.
        var createRequest = new CreateAppDto
        {
            Name = appName,
            // The template is just referenced by the name.
            Template = template.Name
        };

        var (app, _) = await _.PostAppAsync(createRequest);


        // STEP 3: Get schemas.
        var schemas = await app.Schemas.GetSchemasAsync();

        Assert.NotEmpty(schemas.Items);

        await Verify(schemas);
    }
}
