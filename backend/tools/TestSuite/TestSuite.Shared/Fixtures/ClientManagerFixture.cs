// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Configuration;

namespace TestSuite.Fixtures
{
    public class ClientManagerFixture : IDisposable
    {
        public string AppName { get; } = GetValue("APP__NAME", "integration-tests");

        public string ClientId { get; } = GetValue("CLIENT__ID", "root");

        public string ClientSecret { get; } = GetValue("CLIENT__SECRET", "xeLd6jFxqbXJrfmNLlO2j1apagGGGSyZJhFnIuHp4I0=");

        public string ServerUrl { get; } = GetValue("SERVER__URL", "https://localhost:5001");

        public SquidexClientManager ClientManager { get; }

        public ClientManagerFixture()
        {
            ClientManager = new SquidexClientManager(new SquidexOptions
            {
                AppName = AppName,
                ClientId = ClientId,
                ClientSecret = ClientSecret,
                Configurator = AcceptAllCertificatesConfigurator.Instance,
                Url = ServerUrl
            });

            if (TryGetTimeout(out var waitSeconds))
            {
                Task.Run(async () =>
                {
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
                                continue;
                            }
                        }
                    }
                }).Wait();
            }
        }

        private static bool TryGetTimeout(out int timeout)
        {
            return int.TryParse(Environment.GetEnvironmentVariable("CONFIG__WAIT"), out timeout) && timeout > 10;
        }

        private static string GetValue(string name, string defaultValue)
        {
            var variable = Environment.GetEnvironmentVariable($"CONFIG__{name}");

            if (!string.IsNullOrWhiteSpace(variable))
            {
                return variable;
            }

            return defaultValue;
        }

        public virtual void Dispose()
        {
        }
    }
}
