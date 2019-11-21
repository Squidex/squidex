// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure.Reflection;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Assets.Models
{
    public sealed class AssetFolderDto : Resource
    {
        /// <summary>
        /// The id of the asset.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The id of the parent folder. Empty for files without parent.
        /// </summary>
        public Guid ParentId { get; set; }

        /// <summary>
        /// The folder name.
        /// </summary>
        [Required]
        public string FolderName { get; set; }

        /// <summary>
        /// The version of the asset folder.
        /// </summary>
        public long Version { get; set; }

        public static AssetFolderDto FromAssetFolder(IAssetFolderEntity asset, ApiController controller, string app)
        {
            var response = SimpleMapper.Map(asset, new AssetFolderDto());

            return CreateLinks(response, controller, app);
        }

        private static AssetFolderDto CreateLinks(AssetFolderDto response, ApiController controller, string app)
        {
            var values = new { app, id = response.Id };

            response.AddSelfLink(controller.Url<AssetsController>(x => nameof(x.GetAsset), values));

            if (controller.HasPermission(Permissions.AppAssetsUpdate))
            {
                response.AddPutLink("update", controller.Url<AssetFoldersController>(x => nameof(x.PutAssetFolder), values));

                response.AddPutLink("move", controller.Url<AssetFoldersController>(x => nameof(x.PutAssetFolderParent), values));
            }

            if (controller.HasPermission(Permissions.AppAssetsUpdate))
            {
                response.AddDeleteLink("delete", controller.Url<AssetFoldersController>(x => nameof(x.DeleteAssetFolder), values));
            }

            return response;
        }
    }
}
