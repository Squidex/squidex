// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net.Http;
using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Configuration;

namespace TestSuite.Fixtures
{
    public class ClientManagerFixture : IDisposable
    {
        public string ServerUrl { get; } = "https://localhost:5001";

        public string ClientId { get; } = "root";

        public string ClientSecret { get; } = "xeLd6jFxqbXJrfmNLlO2j1apagGGGSyZJhFnIuHp4I0=";

        public string AppName { get; } = "integration-tests";

        public SquidexClientManager ClientManager { get; }

        public sealed class Configurator : IHttpConfigurator
        {
            public void Configure(HttpClient httpClient)
            {
            }

            public void Configure(HttpClientHandler httpClientHandler)
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, certificate, chain, error) => true;
            }
        }

        public ClientManagerFixture()
        {
            ClientManager = new SquidexClientManager(new SquidexOptions
            {
                AppName = AppName,
                ClientId = ClientId,
                ClientSecret = ClientSecret,
                Configurator = new Configurator(),
                ReadResponseAsString = true,
                Url = ServerUrl
            });
        }

        public virtual void Dispose()
        {
        }
    }
}
