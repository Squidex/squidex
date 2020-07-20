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
        public IAppsClient Apps => Squidex.Apps;

        public IBackupsClient Backups => Squidex.Backups;

        public ILanguagesClient Languages => Squidex.Languages;

        public IPingClient Ping => Squidex.Ping;

        public IRulesClient Rules => Squidex.Rules;

        public ISchemasClient Schemas => Squidex.Schemas;
    }
}
