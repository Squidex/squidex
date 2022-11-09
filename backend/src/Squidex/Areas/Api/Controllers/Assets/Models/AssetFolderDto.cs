// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Assets.Models;

public sealed class AssetFolderDto : Resource
{
    /// <summary>
    /// The ID of the asset.
    /// </summary>
    public DomainId Id { get; set; }

    /// <summary>
    /// The ID of the parent folder. Empty for files without parent.
    /// </summary>
    public DomainId ParentId { get; set; }

    /// <summary>
    /// The folder name.
    /// </summary>
    [LocalizedRequired]
    public string FolderName { get; set; }

    /// <summary>
    /// The version of the asset folder.
    /// </summary>
    public long Version { get; set; }

    public static AssetFolderDto FromDomain(IAssetFolderEntity asset, Resources resources)
    {
        var result = SimpleMapper.Map(asset, new AssetFolderDto());

        return result.CreateLinks(resources);
    }

    private AssetFolderDto CreateLinks(Resources resources)
    {
        var values = new { app = resources.App, id = Id };

        if (resources.CanUpdateAsset)
        {
            AddPutLink("update",
                resources.Url<AssetFoldersController>(x => nameof(x.PutAssetFolder), values));

            AddPutLink("move",
                resources.Url<AssetFoldersController>(x => nameof(x.PutAssetFolderParent), values));
        }

        if (resources.CanUpdateAsset)
        {
            AddDeleteLink("delete",
                resources.Url<AssetFoldersController>(x => nameof(x.DeleteAssetFolder), values));
        }

        return this;
    }
}
