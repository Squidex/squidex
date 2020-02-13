// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
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
            Guard.NotNull(urlGenerator);

            this.urlGenerator = urlGenerator;
        }

        public Task<SearchResults> SearchAsync(string query, Context context)
        {
            var result = new SearchResults();

            var appId = context.App.NamedId();

            void Search(string term, string permissionId, Func<NamedId<Guid>, string> generate, SearchResultType? type = null)
            {
                if (result.Count < MaxItems && term.Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    var permission = Permissions.ForApp(permissionId, appId.Name);

                    if (context.Permissions.Allows(permission))
                    {
                        var url = generate(appId);

                        result.Add(term, type ?? SearchResultType.Setting, url);
                    }
                }
            }

            Search("Assets", Permissions.AppAssetsRead, appId => urlGenerator.AssetsUI(appId), SearchResultType.Asset);
            Search("Backups", Permissions.AppBackupsRead, urlGenerator.BackupsUI);
            Search("Clients", Permissions.AppClientsRead, urlGenerator.ClientsUI);
            Search("Contents", Permissions.AppCommon, urlGenerator.ContentsUI, SearchResultType.Content);
            Search("Contributors", Permissions.AppContributorsRead, urlGenerator.ContributorsUI);
            Search("Dashboard", Permissions.AppCommon, urlGenerator.DashboardUI);
            Search("Patterns", Permissions.AppCommon, urlGenerator.PatternsUI);
            Search("Languages", Permissions.AppRolesRead, urlGenerator.RulesUI);
            Search("Roles", Permissions.AppRolesRead, urlGenerator.RulesUI);
            Search("Rules", Permissions.AppRulesRead, urlGenerator.RulesUI, SearchResultType.Rule);
            Search("Schemas", Permissions.AppCommon, urlGenerator.SchemasUI, SearchResultType.Schema);
            Search("Subscription", Permissions.AppPlansRead, urlGenerator.PlansUI);
            Search("Workflows", Permissions.AppWorkflowsRead, urlGenerator.WorkflowsUI);

            return Task.FromResult(result);
        }
    }
}
