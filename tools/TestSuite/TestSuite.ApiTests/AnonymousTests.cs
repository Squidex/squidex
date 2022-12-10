// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using Squidex.ClientLibrary.Management;
using TestSuite.Fixtures;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

[UsesVerify]
public class AnonymousTests : IClassFixture<ClientFixture>
{
    public ClientFixture _ { get; }

    public AnonymousTests(ClientFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_create_app_with_anonymous_read_access()
    {
        var appName = Guid.NewGuid().ToString();

        // STEP 1: Create app
        var createRequest = new CreateAppDto
        {
            Name = appName
        };

        var app = await _.Apps.PostAppAsync(createRequest);

        // Should return create app with correct name.
        Assert.Equal(appName, app.Name);


        // STEP 2: Make the client anonymous.
        var clientRequest = new UpdateClientDto
        {
            AllowAnonymous = true
        };

        await _.Apps.PutClientAsync(appName, "default", clientRequest);


        // STEP 3: Check anonymous permission
        var url = $"{_.ClientManager.Options.Url}api/apps/{appName}/settings";

        using (var httpClient = new HttpClient())
        {
            var response = await httpClient.GetAsync(url);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        await Verify(app);
    }

    [Fact]
    public async Task Should_create_app_with_anonymous_write_access()
    {
        var appName = Guid.NewGuid().ToString();

        // STEP 1: Create app
        var createRequest = new CreateAppDto
        {
            Name = appName
        };

        var app = await _.Apps.PostAppAsync(createRequest);

        // Should return create app with correct name.
        Assert.Equal(appName, app.Name);


        // STEP 2: Make the client anonymous.
        var clientRequest = new UpdateClientDto
        {
            AllowAnonymous = true
        };

        await _.Apps.PutClientAsync(appName, "default", clientRequest);


        // STEP 3: Create schema
        var schemaRequest = new CreateSchemaDto
        {
            Name = "my-content",
            // Schema must be published to create content.
            IsPublished = true
        };

        await _.Schemas.PostSchemaAsync(appName, schemaRequest);


        // STEP 3: Create a content.
        var url = $"{_.ClientManager.Options.Url}api/content/{appName}/my-content";

        using (var httpClient = new HttpClient())
        {
            var response = await httpClient.PostAsync(url, new StringContent("{}", null, "text/json"));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        await Verify(app);
    }
}
