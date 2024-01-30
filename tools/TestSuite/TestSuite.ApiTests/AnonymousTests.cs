// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using Squidex.ClientLibrary;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class AnonymousTests : IClassFixture<ClientFixture>
{
    private readonly string appName = Guid.NewGuid().ToString();

    public ClientFixture _ { get; }

    public AnonymousTests(ClientFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_create_app_with_anonymous_read_access()
    {
        // STEP 1: Create app.
        var (app, dto) = await _.PostAppAsync(appName);

        // Should return create app with correct name.
        Assert.Equal(appName, app.Options.AppName);


        // STEP 2: Make the client anonymous.
        var clientRequest = new UpdateClientDto
        {
            AllowAnonymous = true
        };

        await app.Apps.PutClientAsync("default", clientRequest);


        // STEP 3: Check anonymous permission.
        var url = $"{_.Client.Options.Url}api/apps/{app.Options.AppName}/settings";

        using (var httpClient = new HttpClient())
        {
            var response = await httpClient.GetAsync(url);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        await Verify(dto);
    }

    [Fact]
    public async Task Should_create_app_with_anonymous_write_access()
    {
        // STEP 1: Create app.
        var (app, dto) = await _.PostAppAsync(appName);

        // Should return create app with correct name.
        Assert.Equal(appName, app.Options.AppName);


        // STEP 2: Make the client anonymous.
        var clientRequest = new UpdateClientDto
        {
            AllowAnonymous = true
        };

        await app.Apps.PutClientAsync("default", clientRequest);


        // STEP 3: Create schema.
        var schemaRequest = new CreateSchemaDto
        {
            Name = "my-content",
            // Schema must be published to create content.
            IsPublished = true
        };

        await app.Schemas.PostSchemaAsync(schemaRequest);


        // STEP 3: Create a content.
        var url = $"{_.Client.Options.Url}api/content/{app.Options.AppName}/my-content";

        using (var httpClient = new HttpClient())
        {
            var response = await httpClient.PostAsync(url, new StringContent("{}", null, "text/json"));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        await Verify(dto);
    }
}
