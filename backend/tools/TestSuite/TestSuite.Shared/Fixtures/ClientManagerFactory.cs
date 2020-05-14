// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Configuration;

namespace TestSuite.Fixtures
{
    public static class ClientManagerFactory
    {
        private static Task<SquidexClientManager> manager;

        public static Task<SquidexClientManager> CreateAsync()
        {
            if (manager == null)
            {
                manager = CreateInternalAsync();
            }

            return manager;
        }

        private static async Task<SquidexClientManager> CreateInternalAsync()
        {
            var appName = GetValue("APP__NAME", "integration-tests");
            var clientId = GetValue("CLIENT__ID", "root");
            var clientSecret = GetValue("CLIENT__SECRET", "xeLd6jFxqbXJrfmNLlO2j1apagGGGSyZJhFnIuHp4I0=");
            var serviceURl = GetValue("SERVER__URL", "https://localhost:5001");

            var clientManager = new SquidexClientManager(new SquidexOptions
            {
                AppName = appName,
                ClientId = clientId,
                ClientSecret = clientSecret,
                ReadResponseAsString = true,
                Configurator = AcceptAllCertificatesConfigurator.Instance,
                Url = serviceURl
            });

            if (TryGetTimeout(out var waitSeconds))
            {
                Console.WriteLine("Waiting {0} seconds to access server", waitSeconds);

                var pingClient = clientManager.CreatePingClient();

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
                            await Task.Delay(waitSeconds / 100);
                        }
                    }
                }
            }

            return clientManager;
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
