// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Assets.Models
{
    public sealed class AssetFoldersDto : Resource
    {
        /// <summary>
        /// The total number of assets.
        /// </summary>
        public long Total { get; set; }

        /// <summary>
        /// The assets folders.
        /// </summary>
        [LocalizedRequired]
        public AssetFolderDto[] Items { get; set; }

        /// <summary>
        /// The path to the current folder.
        /// </summary>
        [LocalizedRequired]
        public AssetFolderDto[] Path { get; set; }

        public static AssetFoldersDto FromAssets(IResultList<IAssetFolderEntity> assetFolders, IEnumerable<IAssetFolderEntity> path, Resources resources)
        {
            var response = new AssetFoldersDto
            {
                Total = assetFolders.Total,
                Items = assetFolders.Select(x => AssetFolderDto.FromAssetFolder(x, resources)).ToArray()
            };

            response.Path = path.Select(x => AssetFolderDto.FromAssetFolder(x, resources)).ToArray();

            return CreateLinks(response, resources);
        }

        private static AssetFoldersDto CreateLinks(AssetFoldersDto response, Resources resources)
        {
            var values = new { app = resources.App };

            response.AddSelfLink(resources.Url<AssetFoldersController>(x => nameof(x.GetAssetFolders), values));

            if (resources.CanUpdateAsset)
            {
                response.AddPostLink("create", resources.Url<AssetFoldersController>(x => nameof(x.PostAssetFolder), values));
            }

            return response;
        }
    }
}
