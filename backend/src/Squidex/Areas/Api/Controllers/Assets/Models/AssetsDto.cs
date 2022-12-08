// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Assets.Models;

public sealed class AssetsDto : Resource
{
    /// <summary>
    /// The total number of assets.
    /// </summary>
    public long Total { get; set; }

    /// <summary>
    /// The assets.
    /// </summary>
    [LocalizedRequired]
    public AssetDto[] Items { get; set; }

    public static AssetsDto FromDomain(IResultList<IEnrichedAssetEntity> assets, Resources resources)
    {
        var result = new AssetsDto
        {
            Total = assets.Total,
            Items = assets.Select(x => AssetDto.FromDomain(x, resources)).ToArray()
        };

        return result.CreateLinks(resources);
    }

    private AssetsDto CreateLinks(Resources resources)
    {
        var values = new { app = resources.App };

        AddSelfLink(resources.Url<AssetsController>(x => nameof(x.GetAssets), values));

        if (resources.CanCreateAsset)
        {
            AddPostLink("create",
                resources.Url<AssetsController>(x => nameof(x.PostAsset), values));
        }

        if (resources.CanUpdateAsset)
        {
            var tagValue = new { values.app, name = "tag" };

            AddPutLink("tags/rename",
                resources.Url<AssetsController>(x => nameof(x.PutTag), tagValue));
        }

        AddGetLink("tags",
            resources.Url<AssetsController>(x => nameof(x.GetTags), values));

        return this;
    }
}
