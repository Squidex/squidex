// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.ClientLibrary;
using TestSuite.Utils;

namespace TestSuite;

public sealed class ClientManagerWrapper
{
    public ISquidexClientManager ClientManager { get; }

    public ClientManagerWrapper()
    {
        var services =
            new ServiceCollection()
                .AddSquidexClient(options =>
                {
                    options.AppName = TestHelpers.GetAndPrintValue("config:app:name", "integration-tests");
                    options.ClientId = TestHelpers.GetAndPrintValue("config:client:id", "root");
                    options.ClientSecret = TestHelpers.GetAndPrintValue("config:client:secret", "xeLd6jFxqbXJrfmNLlO2j1apagGGGSyZJhFnIuHp4I0=");
                    options.Url = TestHelpers.GetAndPrintValue("config:server:url", "https://localhost:5001");
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

        ClientManager = services.GetRequiredService<ISquidexClientManager>();
    }

    public async Task<ClientManagerWrapper> ConnectAsync()
    {
        var waitSeconds = TestHelpers.Configuration.GetValue<int>("config:wait");

        if (waitSeconds > 10)
        {
            Console.WriteLine("Waiting {0} seconds to access server", waitSeconds);

            var pingClient = ClientManager.CreatePingClient();
            try
            {
                using (var cts = new CancellationTokenSource(waitSeconds * 1000))
                {
                    while (!cts.IsCancellationRequested)
                    {
                        try
                        {
                            await pingClient.GetPingAsync(cts.Token);
                            break;
                        }
                        catch
                        {
                            await Task.Delay(100, cts.Token);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw new InvalidOperationException("Cannot connect to test system.");
            }

            Console.WriteLine("Connected to server.");
        }
        else
        {
            Console.WriteLine("Waiting for server is skipped.");
        }

        return this;
    }
}
