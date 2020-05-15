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
        public string AppName => ClientManager.Options.AppName;

        public string ClientId => ClientManager.Options.ClientId;

        public string ClientSecret => ClientManager.Options.ClientSecret;

        public string ServerUrl => ClientManager.Options.Url;

        public SquidexClientManager ClientManager { get; }

        public ClientManagerFixture()
        {
            ClientManager = ClientManagerFactory.CreateAsync().Result;
        }

        public virtual void Dispose()
        {
        }
    }
}
