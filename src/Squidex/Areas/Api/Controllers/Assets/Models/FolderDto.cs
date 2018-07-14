// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;

namespace Squidex.Areas.Api.Controllers.Assets.Models
{
    public class FolderDto : AssetsDto
    {
        /// <summary>
        /// The path to the folder.
        /// </summary>
        public FolderPathItem[] Path { get; set; }

        public static FolderDto FromAssets(IResultList<IAssetEntity> assets, FolderPathItem[] path)
        {
            return new FolderDto { Total = assets.Total, Items = assets.Select(AssetDto.FromAsset).ToArray(), Path = path };
        }
    }
}
