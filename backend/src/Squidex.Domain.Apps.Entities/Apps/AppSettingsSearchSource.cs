// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities.Search;
using Squidex.Infrastructure;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class AppSettingsSearchSource : ISearchSource
    {
        private const int MaxItems = 3;
        private readonly IUrlGenerator urlGenerator;

        public AppSettingsSearchSource(IUrlGenerator urlGenerator)
        {
            this.urlGenerator = urlGenerator;
        }

        public Task<SearchResults> SearchAsync(string query, Context context,
            CancellationToken ct)
        {
            var result = new SearchResults();

            var appId = context.App.NamedId();

            void Search(string term, string permissionId, Func<NamedId<DomainId>, string> generate, SearchResultType type)
            {
                if (result.Count < MaxItems && term.Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    if (context.Allows(permissionId))
                    {
                        var url = generate(appId);

                        result.Add(term, type, url);
                    }
                }
            }

            Search("Assets", Permissions.AppAssetsRead,
                a => urlGenerator.AssetsUI(a), SearchResultType.Asset);

            Search("Backups", Permissions.AppBackupsRead,
                urlGenerator.BackupsUI, SearchResultType.Setting);

            Search("Clients", Permissions.AppClientsRead,
                urlGenerator.ClientsUI, SearchResultType.Setting);

            Search("Contributors", Permissions.AppContributorsRead,
                urlGenerator.ContributorsUI, SearchResultType.Setting);

            Search("Dashboard", Permissions.AppUsage,
                urlGenerator.DashboardUI, SearchResultType.Dashboard);

            Search("Languages", Permissions.AppLanguagesRead,
                urlGenerator.LanguagesUI, SearchResultType.Setting);

            Search("Roles", Permissions.AppRolesRead,
                urlGenerator.RolesUI, SearchResultType.Setting);

            Search("Rules", Permissions.AppRulesRead,
                urlGenerator.RulesUI, SearchResultType.Rule);

            Search("Schemas", Permissions.AppSchemasRead,
                urlGenerator.SchemasUI, SearchResultType.Schema);

            Search("Subscription", Permissions.AppPlansRead,
                urlGenerator.PlansUI, SearchResultType.Setting);

            Search("Workflows", Permissions.AppWorkflowsRead,
                urlGenerator.WorkflowsUI, SearchResultType.Setting);

            return Task.FromResult(result);
        }
    }
}
