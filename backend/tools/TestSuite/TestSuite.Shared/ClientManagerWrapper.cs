// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Lazy;
using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Configuration;
using Squidex.ClientLibrary.Management;

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
            var appName = GetValue("APP__NAME", "integration-tests");
            var clientId = GetValue("CLIENT__ID", "root");
            var clientSecret = GetValue("CLIENT__SECRET", "xeLd6jFxqbXJrfmNLlO2j1apagGGGSyZJhFnIuHp4I0=");
            var serviceURl = GetValue("SERVER__URL", "https://localhost:5001");

            ClientManager = new SquidexClientManager(new SquidexOptions
            {
                AppName = appName,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Configurator = AcceptAllCertificatesConfigurator.Instance,
                ReadResponseAsString = true,
                Url = serviceURl
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
            if (TryGetTimeout(out var waitSeconds))
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

        private static bool TryGetTimeout(out int timeout)
        {
            var variable = Environment.GetEnvironmentVariable("CONFIG__WAIT");

            if (!string.IsNullOrWhiteSpace(variable))
            {
                Console.WriteLine("Using: CONFIG__WAIT={0}", variable);
            }

            return int.TryParse(variable, out timeout) && timeout > 10;
        }

        private static string GetValue(string name, string defaultValue)
        {
            var variable = Environment.GetEnvironmentVariable($"CONFIG__{name}");

            if (!string.IsNullOrWhiteSpace(variable))
            {
                Console.WriteLine("Using: {0}={1}", name, variable);

                return variable;
            }

            return defaultValue;
        }
    }
}
