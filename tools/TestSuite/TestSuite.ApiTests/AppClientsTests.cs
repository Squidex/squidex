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
public sealed class AppClientsTests : IClassFixture<ClientFixture>
{
    private readonly string appName = Guid.NewGuid().ToString();
    private readonly string id = Guid.NewGuid().ToString();
    private readonly string clientRole = "Editor";
    private readonly string clientName = "My Client";

    public ClientFixture _ { get; }

    public AppClientsTests(ClientFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_create_client()
    {
        // STEP 0: Create app.
        await CreateAppAsync();


        // STEP 1: Create client.
        var client = await CreateAsync();

        // Should return client with correct name and id.
        Assert.Equal(clientRole, client.Role);
        Assert.Equal(id, client.Name);

        await Verify(client)
            .IgnoreMember<ClientDto>(x => x.Secret);
    }

    [Fact]
    public async Task Should_update_client()
    {
        // STEP 0: Create app.
        await CreateAppAsync();


        // STEP 0: Create client.
        var client = await CreateAsync();


        // STEP 1: Update client name.
        var updateNameRequest = new UpdateClientDto
        {
            Name = clientName,
            AllowAnonymous = true,
            ApiCallsLimit = 100,
            ApiTrafficLimit = 200,
            Role = "Owner"
        };

        var clients_2 = await _.Apps.PutClientAsync(appName, client.Id, updateNameRequest);
        var client_2 = clients_2.Items.Find(x => x.Id == client.Id);

        // Should update client name.
        Assert.Equal(updateNameRequest.Name, client_2.Name);
        Assert.Equal(updateNameRequest.AllowAnonymous, client_2.AllowAnonymous);
        Assert.Equal(updateNameRequest.ApiCallsLimit, client_2.ApiCallsLimit);
        Assert.Equal(updateNameRequest.ApiTrafficLimit, client_2.ApiTrafficLimit);
        Assert.Equal(updateNameRequest.Role, client_2.Role);

        await Verify(clients_2)
            .IgnoreMember<ClientDto>(x => x.Secret);
    }

    [Fact]
    public async Task Should_delete_client()
    {
        // STEP 0: Create app.
        await CreateAppAsync();


        // STEP 0: Create client.
        var client = await CreateAsync();


        // STEP 1: Delete client
        var clients_2 = await _.Apps.DeleteClientAsync(appName, client.Id);

        // Should not return deleted client.
        Assert.DoesNotContain(clients_2.Items, x => x.Id == client.Id);

        await Verify(clients_2)
            .IgnoreMember<ClientDto>(x => x.Secret);
    }

    private async Task<ClientDto> CreateAsync()
    {
        var createRequest = new CreateClientDto
        {
            Id = id
        };

        var clients = await _.Apps.PostClientAsync(appName, createRequest);
        var client = clients.Items.Find(x => x.Id == id);

        return client;
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
