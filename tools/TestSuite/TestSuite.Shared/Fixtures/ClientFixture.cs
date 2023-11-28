// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Squidex.ClientLibrary;
using Xunit;

namespace TestSuite.Fixtures;

public class ClientFixture : IAsyncLifetime
{
    public ClientWrapper Squidex { get; private set; }

    public string AppName => Client.Options.AppName;

    public string ClientId => Client.Options.ClientId;

    public string ClientSecret => Client.Options.ClientSecret;

    public string Url => Client.Options.Url;

    public ISquidexClient Client => Squidex.Client;

    static ClientFixture()
    {
        VerifierSettings.IgnoreMember("AppName");
        VerifierSettings.IgnoreMember("Created");
        VerifierSettings.IgnoreMember("CreatedBy");
        VerifierSettings.IgnoreMember("EditToken");
        VerifierSettings.IgnoreMember("Href");
        VerifierSettings.IgnoreMember("LastModified");
        VerifierSettings.IgnoreMember("LastModifiedBy");
        VerifierSettings.IgnoreMember("RoleProperties");
        VerifierSettings.IgnoreMember("SchemaName");
        VerifierSettings.IgnoreMembersWithType<DateTimeOffset>();
    }

    public Task<(ISquidexClient, AppDto)> PostAppAsync(string? name = null)
    {
        name ??= Guid.NewGuid().ToString();

        var createRequest = new CreateAppDto
        {
            Name = name
        };

        return PostAppAsync(createRequest);
    }

    public Task<TeamDto> PostTeamAsync(string? name = null)
    {
        name ??= Guid.NewGuid().ToString();

        var request = new CreateTeamDto
        {
            Name = name
        };

        return Client.Teams.PostTeamAsync(request);
    }

    public async Task<(ISquidexClient, AppDto)> PostAppAsync(CreateAppDto request)
    {
        var services =
            new ServiceCollection()
                .AddSquidexClient(options =>
                {
                    options.AppName = request.Name;
                    options.ClientId = ClientId;
                    options.ClientSecret = ClientSecret;
                    options.Url = Client.Options.Url;
                    options.ReadResponseAsString = true;
                })
                .AddSquidexHttpClient()
                    .ConfigurePrimaryHttpMessageHandler(() =>
                    {
                        return new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                        };
                    }).Services
                .BuildServiceProvider();

        var client = services.GetRequiredService<ISquidexClient>();

        return (client, await client.Apps.PostAppAsync(request));
    }

    public virtual async Task InitializeAsync()
    {
        Squidex = await Factories.CreateAsync(nameof(ClientWrapper), async () =>
        {
            var clientManager = new ClientWrapper();

            await clientManager.ConnectAsync();

            return clientManager;
        });
    }

    public virtual Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
