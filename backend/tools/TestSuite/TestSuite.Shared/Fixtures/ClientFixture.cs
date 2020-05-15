// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary.Management;

namespace TestSuite.Fixtures
{
    public class ClientFixture : ClientManagerFixture
    {
        public IAppsClient Apps { get; }

        public IBackupsClient Backups { get; }

        public ILanguagesClient Languages { get; }

        public IPingClient Ping { get; }

        public IRulesClient Rules { get; }

        public ISchemasClient Schemas { get; }

        public ClientFixture()
        {
            Apps = ClientManager.CreateAppsClient();

            Backups = ClientManager.CreateBackupsClient();

            Languages = ClientManager.CreateLanguagesClient();

            Languages = ClientManager.CreateLanguagesClient();

            Ping = ClientManager.CreatePingClient();

            Rules = ClientManager.CreateRulesClient();

            Schemas = ClientManager.CreateSchemasClient();
        }
    }
}
