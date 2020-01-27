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
        public IAppsClient Apps => ClientManager.CreateAppsClient();

        public IBackupsClient Backups => ClientManager.CreateBackupsClient();

        public ILanguagesClient Languages => ClientManager.CreateLanguagesClient();

        public IPingClient Ping => ClientManager.CreatePingClient();

        public IRulesClient Rules => ClientManager.CreateRulesClient();

        public ISchemasClient Schemas => ClientManager.CreateSchemasClient();
    }
}
