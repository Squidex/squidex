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

        public IAssetsClient Assets => Squidex.Assets;

        public IBackupsClient Backups => Squidex.Backups;

        public ICommentsClient Comments => Squidex.Comments;

        public IDiagnosticsClient Diagnostics => Squidex.Diagnostics;

        public IHistoryClient History => Squidex.History;

        public ILanguagesClient Languages => Squidex.Languages;

        public IPingClient Ping => Squidex.Ping;

        public IPlansClient Plans => Squidex.Plans;

        public IRulesClient Rules => Squidex.Rules;

        public ISchemasClient Schemas => Squidex.Schemas;

        public ISearchClient Search => Squidex.Search;

        public ITemplatesClient Templates => Squidex.Templates;

        public ITranslationsClient Translations => Squidex.Translations;
    }
}
