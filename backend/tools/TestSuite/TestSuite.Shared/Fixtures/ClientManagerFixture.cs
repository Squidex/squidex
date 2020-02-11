// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.ClientLibrary;

namespace TestSuite.Fixtures
{
    public class ClientManagerFixture : IDisposable
    {
        public string ServerUrl { get; } = "http://localhost:5000";

        public string ClientId { get; } = "root";

        public string ClientSecret { get; } = "xeLd6jFxqbXJrfmNLlO2j1apagGGGSyZJhFnIuHp4I0=";

        public string AppName { get; } = "integration-tests";

        public SquidexClientManager ClientManager { get; }

        public ClientManagerFixture()
        {
            ClientManager = new SquidexClientManager(new SquidexOptions
            {
                AppName = AppName,
                ClientId = ClientId,
                ClientSecret = ClientSecret,
                ReadResponseAsString = true,
                Url = ServerUrl
            });
        }

        public virtual void Dispose()
        {
        }
    }
}
