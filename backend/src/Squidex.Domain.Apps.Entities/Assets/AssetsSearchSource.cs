// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Search;
using Squidex.Infrastructure.Queries;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Assets;

public sealed class AssetsSearchSource : ISearchSource
{
    private readonly IAssetQueryService assetQuery;
    private readonly IUrlGenerator urlGenerator;

    public AssetsSearchSource(IAssetQueryService assetQuery, IUrlGenerator urlGenerator)
    {
        this.assetQuery = assetQuery;

        this.urlGenerator = urlGenerator;
    }

    public async Task<SearchResults> SearchAsync(string query, Context context,
        CancellationToken ct)
    {
        var result = new SearchResults();

        if (context.UserPermissions.Allows(PermissionIds.AppAssetsRead, context.App.Name))
        {
            var filter = ClrFilter.Contains("fileName", query);

            var clrQuery = new ClrQuery { Filter = filter, Take = 5 };

            var assets = await assetQuery.QueryAsync(context, null, Q.Empty.WithQuery(clrQuery), ct);

            if (assets.Count > 0)
            {
                foreach (var asset in assets)
                {
                    var url = urlGenerator.AssetsUI(context.App.NamedId(), asset.Id.ToString());

                    result.Add(asset.FileName, SearchResultType.Asset, url);
                }
            }
        }

        return result;
    }
}
