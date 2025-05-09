﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Assets.Models;

public sealed class AssetFoldersDto : Resource
{
    /// <summary>
    /// The total number of assets.
    /// </summary>
    public long Total { get; set; }

    /// <summary>
    /// The assets folders.
    /// </summary>
    public AssetFolderDto[] Items { get; set; }

    /// <summary>
    /// The path to the current folder.
    /// </summary>
    public AssetFolderDto[] Path { get; set; }

    public static AssetFoldersDto FromDomain(IResultList<AssetFolder> assetFolders, IEnumerable<AssetFolder> path, Resources resources)
    {
        var result = new AssetFoldersDto
        {
            Total = assetFolders.Total,
            Items = assetFolders.Select(x => AssetFolderDto.FromDomain(x, resources)).ToArray(),
            Path = path.Select(x => AssetFolderDto.FromDomain(x, resources)).ToArray(),
        };

        return result.CreateLinks(resources);
    }

    private AssetFoldersDto CreateLinks(Resources resources)
    {
        var values = new { app = resources.App };

        AddSelfLink(resources.Url<AssetFoldersController>(x => nameof(x.GetAssetFolders), values));

        if (resources.CanUpdateAsset)
        {
            AddPostLink("create",
                resources.Url<AssetFoldersController>(x => nameof(x.PostAssetFolder), values));
        }

        return this;
    }
}
