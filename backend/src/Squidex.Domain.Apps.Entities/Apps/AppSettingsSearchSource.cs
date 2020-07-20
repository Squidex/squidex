﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
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
            Guard.NotNull(urlGenerator, nameof(urlGenerator));

            this.urlGenerator = urlGenerator;
        }

        public Task<SearchResults> SearchAsync(string query, Context context)
        {
            var result = new SearchResults();

            var appId = context.App.NamedId();

            void Search(string term, string permissionId, Func<NamedId<DomainId>, string> generate, SearchResultType type)
            {
                if (result.Count < MaxItems && term.Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    var permission = Permissions.ForApp(permissionId, appId.Name);

                    if (context.Permissions.Allows(permission))
                    {
                        var url = generate(appId);

                        result.Add(term, type, url);
                    }
                }
            }

            Search("Assets", Permissions.AppAssetsRead,
                urlGenerator.AssetsUI, SearchResultType.Asset);

            Search("Backups", Permissions.AppBackupsRead,
                urlGenerator.BackupsUI, SearchResultType.Setting);

            Search("Clients", Permissions.AppClientsRead,
                urlGenerator.ClientsUI, SearchResultType.Setting);

            Search("Contents", Permissions.AppCommon,
                urlGenerator.ContentsUI, SearchResultType.Content);

            Search("Contributors", Permissions.AppContributorsRead,
                urlGenerator.ContributorsUI, SearchResultType.Setting);

            Search("Dashboard", Permissions.AppCommon,
                urlGenerator.DashboardUI, SearchResultType.Dashboard);

            Search("Languages", Permissions.AppCommon,
                urlGenerator.LanguagesUI, SearchResultType.Setting);

            Search("Patterns", Permissions.AppCommon,
                urlGenerator.PatternsUI, SearchResultType.Setting);

            Search("Roles", Permissions.AppRolesRead,
                urlGenerator.RolesUI, SearchResultType.Setting);

            Search("Rules", Permissions.AppRulesRead,
                urlGenerator.RulesUI, SearchResultType.Rule);

            Search("Schemas", Permissions.AppCommon,
                urlGenerator.SchemasUI, SearchResultType.Schema);

            Search("Subscription", Permissions.AppPlansRead,
                urlGenerator.PlansUI, SearchResultType.Setting);

            Search("Workflows", Permissions.AppWorkflowsRead,
                urlGenerator.WorkflowsUI, SearchResultType.Setting);

            return Task.FromResult(result);
        }
    }
}