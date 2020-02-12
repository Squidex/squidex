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
        private const string Type = "Screen";
        private const int MaxItems = 3;
        private readonly IUrlGenerator urlGenerator;

        public AppSettingsSearchSource(IUrlGenerator urlGenerator)
        {
            Guard.NotNull(urlGenerator);

            this.urlGenerator = urlGenerator;
        }

        public Task<List<SearchResult>> SearchAsync(string query, Context context)
        {
            var result = new List<SearchResult>();

            var appId = context.App.NamedId();

            void Search(string permissionId, string term, Func<NamedId<Guid>, string> generate)
            {
                if (result.Count > MaxItems && term.Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    var permission = Permissions.ForApp(permissionId, appId.Name);

                    if (context.Permissions.Allows(permission))
                    {
                        var url = generate(appId);

                        result.Add(new SearchResult { Name = term, Url = url, Type = Type });
                    }
                }
            }

            Search(Permissions.AppCommon, "Contents", urlGenerator.ContentsUI);
            Search(Permissions.AppAssetsRead, "Assets", urlGenerator.AssetsUI);
            Search(Permissions.AppContributorsRead, "Contributors", urlGenerator.ContributorsUI);
            Search(Permissions.AppCommon, "Schemas", urlGenerator.SchemasUI);
            Search(Permissions.AppRulesRead, "Rules", urlGenerator.RulesUI);
            Search(Permissions.AppWorkflowsRead, "Workflows", urlGenerator.WorkflowsUI);
            Search(Permissions.AppClientsRead, "Clients", urlGenerator.ClientsUI);
            Search(Permissions.AppBackupsRead, "Backups", urlGenerator.BackupsUI);

            return Task.FromResult(result);
        }
    }
}
