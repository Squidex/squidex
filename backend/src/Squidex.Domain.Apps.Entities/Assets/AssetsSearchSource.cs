// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Search;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class AssetsSearchSource : ISearchSource
    {
        private readonly IAssetQueryService assetQuery;
        private readonly IUrlGenerator urlGenerator;

        public AssetsSearchSource(IAssetQueryService assetQuery, IUrlGenerator urlGenerator)
        {
            Guard.NotNull(assetQuery, nameof(assetQuery));
            Guard.NotNull(urlGenerator, nameof(urlGenerator));

            this.assetQuery = assetQuery;

            this.urlGenerator = urlGenerator;
        }

        public async Task<SearchResults> SearchAsync(string query, Context context)
        {
            var result = new SearchResults();

            var permission = Permissions.ForApp(Permissions.AppAssetsRead, context.App.Name);

            if (context.Permissions.Allows(permission))
            {
                var filter = ClrFilter.Contains("fileName", query);

                var clrQuery = new ClrQuery { Filter = filter, Take = 5 };

                var assets = await assetQuery.QueryAsync(context, null, Q.Empty.WithQuery(clrQuery));

                if (assets.Count > 0)
                {
                    var url = urlGenerator.AssetsUI(context.App.NamedId(), query);

                    foreach (var asset in assets)
                    {
                        result.Add(asset.FileName, SearchResultType.Asset, url);
                    }
                }
            }

            return result;
        }
    }
}
