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

            return response;
        }
    }
}
