// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;
using Squidex.Shared;
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
        [Required]
        public AssetFolderDto[] Items { get; set; }

        public static AssetFoldersDto FromAssets(IResultList<IAssetFolderEntity> assetFolders, ApiController controller, string app)
        {
            var response = new AssetFoldersDto
            {
                Total = assetFolders.Total,
                Items = assetFolders.Select(x => AssetFolderDto.FromAssetFolder(x, controller, app)).ToArray()
            };

            return CreateLinks(response, controller, app);
        }

        private static AssetFoldersDto CreateLinks(AssetFoldersDto response, ApiController controller, string app)
        {
            var values = new { app };

            response.AddSelfLink(controller.Url<AssetFoldersController>(x => nameof(x.GetAssetFolders), values));

            if (controller.HasPermission(Permissions.AppAssetsUpdate))
            {
                response.AddPostLink("create", controller.Url<AssetFoldersController>(x => nameof(x.PostAssetFolder), values));
            }

            return response;
        }
    }
}
