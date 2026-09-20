// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Squidex.ClientLibrary;
using TestSuite.Fixtures;
using TestSuite.Model;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable SA1507 // Code should not contain multiple blank lines in a row

namespace TestSuite.ApiTests;

public class AuthorizationTests(ClientFixture fixture) : IClassFixture<ClientFixture>
{
    public ClientFixture _ { get; } = fixture;

    [Fact]
    public async Task Should_not_write_contents_with_reader_client()
    {
        var (app, _) = await _.PostAppAsync();

        var schemaName = await CreateSchemaAsync(app);

        // STEP 1: Create a client with the reader role.
        var reader = await CreateClientAsync(app, "Reader");


        // STEP 2: Read the contents.
        var contents = await reader.DynamicContents(schemaName).GetAsync();

        Assert.Empty(contents.Items);


        // STEP 3: Write a content and fail.
        var ex = await Assert.ThrowsAnyAsync<SquidexException>(() =>
        {
            var data = new DynamicData
            {
                [TestEntityData.NumberField] = new JObject
                {
                    ["iv"] = 1,
                },
            };

            return reader.DynamicContents(schemaName).CreateAsync(data);
        });

        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public async Task Should_not_read_contents_of_other_schema_with_restricted_client()
    {
        var (app, _) = await _.PostAppAsync();

        var schemaName1 = await CreateSchemaAsync(app);
        var schemaName2 = await CreateSchemaAsync(app);

        // STEP 1: Create a role that can only read one schema.
        var roleName = $"role-{Guid.NewGuid()}";

        await app.Apps.PostRoleAsync(new AddRoleDto { Name = roleName });

        // The permissions of a role are relative to the app.
        var updateRequest = new UpdateRoleDto
        {
            Permissions = [$"contents.{schemaName1}"],
        };

        await app.Apps.PutRoleAsync(roleName, updateRequest);

        var restricted = await CreateClientAsync(app, roleName);


        // STEP 2: Read the contents of the allowed schema.
        var contents = await restricted.DynamicContents(schemaName1).GetAsync();

        Assert.Empty(contents.Items);


        // STEP 3: Read the contents of the other schema and fail.
        var ex = await Assert.ThrowsAnyAsync<SquidexException>(() =>
        {
            return restricted.DynamicContents(schemaName2).GetAsync();
        });

        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public async Task Should_not_write_contents_anonymously_with_read_access()
    {
        var (app, _) = await _.PostAppAsync();

        var schemaName = await CreateSchemaAsync(app);

        // STEP 1: Allow anonymous access with the reader role.
        var clientRequest = new UpdateClientDto
        {
            AllowAnonymous = true,
            Role = "Reader",
        };

        await app.Apps.PutClientAsync("default", clientRequest);


        // STEP 2: Read the contents anonymously.
        var url = $"{_.Client.Options.Url}api/content/{app.Options.AppName}/{schemaName}";

        using var httpClient = new HttpClient();

        var responseRead = await httpClient.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, responseRead.StatusCode);


        // STEP 3: Write a content anonymously and fail.
        var responseWrite = await httpClient.PostAsync(url, new StringContent("{}", null, "text/json"));

        Assert.Equal(HttpStatusCode.Forbidden, responseWrite.StatusCode);
    }

    private async Task<ISquidexClient> CreateClientAsync(ISquidexClient app, string role)
    {
        var id = $"client-{Guid.NewGuid()}";

        var clients = await app.Apps.PostClientAsync(new CreateClientDto { Id = id });
        var client = clients.Items.Find(x => x.Id == id)!;

        await app.Apps.PutClientAsync(id, new UpdateClientDto { Role = role });

        return new ServiceCollection()
            .AddSquidexClient(options =>
            {
                options.AppName = app.Options.AppName;
                options.ClientId = $"{app.Options.AppName}:{id}";
                options.ClientSecret = client.Secret;
                options.Url = _.Client.Options.Url;
                options.ReadResponseAsString = true;
            })
            .AddSquidexHttpClient()
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
                    };
                }).Services
            .BuildServiceProvider()
            .GetRequiredService<ISquidexClient>();
    }

    private static async Task<string> CreateSchemaAsync(ISquidexClient app)
    {
        var schemaName = $"schema-{Guid.NewGuid()}";

        await TestEntity.CreateSchemaAsync(app.Schemas, schemaName);

        return schemaName;
    }
}
