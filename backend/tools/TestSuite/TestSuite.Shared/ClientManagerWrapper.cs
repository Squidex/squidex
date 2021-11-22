// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Lazy;
using Microsoft.Extensions.Configuration;
using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Configuration;
using Squidex.ClientLibrary.Management;
using TestSuite.Utils;

namespace TestSuite
{
    public sealed class ClientManagerWrapper
    {
        private static Task<ClientManagerWrapper> manager;

        public SquidexClientManager ClientManager { get; set; }

        [Lazy]
        public IAppsClient Apps => ClientManager.CreateAppsClient();

        [Lazy]
        public IAssetsClient Assets => ClientManager.CreateAssetsClient();

        [Lazy]
        public IBackupsClient Backups => ClientManager.CreateBackupsClient();

        [Lazy]
        public ILanguagesClient Languages => ClientManager.CreateLanguagesClient();

        [Lazy]
        public IPingClient Ping => ClientManager.CreatePingClient();

        [Lazy]
        public IRulesClient Rules => ClientManager.CreateRulesClient();

        [Lazy]
        public ISchemasClient Schemas => ClientManager.CreateSchemasClient();

        public ClientManagerWrapper()
        {
            static string Variable(string key, string fallback)
            {
                var result = TestHelpers.Configuration[key];

                if (string.IsNullOrWhiteSpace(result))
                {
                    result = fallback;
                }

                return result;
            }

            ClientManager = new SquidexClientManager(new SquidexOptions
            {
                AppName = Variable("app:name", "integration-tests"),
                ClientId = Variable("client:id", "root"),
                ClientSecret = Variable("client:secret", "xeLd6jFxqbXJrfmNLlO2j1apagGGGSyZJhFnIuHp4I0="),
                Configurator = AcceptAllCertificatesConfigurator.Instance,
                ReadResponseAsString = true,
                Url = Variable("service:url", "https://localhost:5001")
            });
        }

        public static Task<ClientManagerWrapper> CreateAsync()
        {
            if (manager == null)
            {
                manager = CreateInternalAsync();
            }

            return manager;
        }

        private static async Task<ClientManagerWrapper> CreateInternalAsync()
        {
            var clientManager = new ClientManagerWrapper();

            await clientManager.ConnectAsync();

            return clientManager;
        }

        public async Task ConnectAsync()
        {
            var waitSeconds = TestHelpers.Configuration.GetValue<int>("config:wait");

            if (waitSeconds > 10)
            {
                Console.WriteLine("Waiting {0} seconds to access server", waitSeconds);

                var pingClient = ClientManager.CreatePingClient();

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
                            await Task.Delay(100);
                        }
                    }
                }
            }
        }
    }
}
