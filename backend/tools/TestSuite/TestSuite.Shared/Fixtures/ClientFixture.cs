// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Management;
using TestSuite.Model;

namespace TestSuite.Fixtures
{
    public class ClientFixture : IDisposable
    {
        public SquidexClientManager ClientManager { get; }

        public IAppsClient Apps { get; }

        public IBackupsClient Backups { get; }

        public string ServerUrl { get; } = TestClient.ServerUrl;

        public ClientFixture()
        {
            ClientManager = TestClient.ClientManager;

            Apps = ClientManager.CreateAppsClient();

            Backups = ClientManager.CreateBackupsClient();
        }

        public virtual void Dispose()
        {
        }
    }
}
