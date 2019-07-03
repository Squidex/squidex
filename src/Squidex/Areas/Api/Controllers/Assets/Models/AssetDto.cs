// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using NodaTime;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Assets.Models
{
    public sealed class AssetDto : Resource
    {
        /// <summary>
        /// The id of the asset.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The file name.
        /// </summary>
        [Required]
        public string FileName { get; set; }

        /// <summary>
        /// The file hash.
        /// </summary>
        [Required]
        public string FileHash { get; set; }

        /// <summary>
        /// The slug.
        /// </summary>
        [Required]
        public string Slug { get; set; }

        /// <summary>
        /// The mime type.
        /// </summary>
        [Required]
        public string MimeType { get; set; }

        /// <summary>
        /// The file type.
        /// </summary>
        [Required]
        public string FileType { get; set; }

        /// <summary>
        /// The asset tags.
        /// </summary>
        public HashSet<string> Tags { get; set; }

        /// <summary>
        /// The size of the file in bytes.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// The version of the file.
        /// </summary>
        public long FileVersion { get; set; }

        /// <summary>
        /// Determines of the created file is an image.
        /// </summary>
        public bool IsImage { get; set; }

        /// <summary>
        /// The width of the image in pixels if the asset is an image.
        /// </summary>
        public int? PixelWidth { get; set; }

        /// <summary>
        /// The height of the image in pixels if the asset is an image.
        /// </summary>
        public int? PixelHeight { get; set; }

        /// <summary>
        /// The user that has created the schema.
        /// </summary>
        [Required]
        public RefToken CreatedBy { get; set; }

        /// <summary>
        /// The user that has updated the asset.
        /// </summary>
        [Required]
        public RefToken LastModifiedBy { get; set; }

        /// <summary>
        /// The date and time when the asset has been created.
        /// </summary>
        public Instant Created { get; set; }

        /// <summary>
        /// The date and time when the asset has been modified last.
        /// </summary>
        public Instant LastModified { get; set; }

        /// <summary>
        /// The version of the asset.
        /// </summary>
        public long Version { get; set; }

        /// <summary>
        /// The metadata.
        /// </summary>
        [JsonProperty("_meta")]
        public AssetMetadata Metadata { get; set; }

        public static AssetDto FromAsset(IEnrichedAssetEntity asset, ApiController controller, string app, bool isDuplicate = false)
        {
            var response = SimpleMapper.Map(asset, new AssetDto { FileType = asset.FileName.FileType() });

            response.Tags = asset.TagNames;

            if (isDuplicate)
            {
                response.Metadata = new AssetMetadata { IsDuplicate = "true" };
            }

            return CreateLinks(response, controller, app);
        }

        private static AssetDto CreateLinks(AssetDto response, ApiController controller, string app)
        {
            var values = new { app, id = response.Id };

            response.AddSelfLink(controller.Url<AssetsController>(x => nameof(x.GetAsset), values));

            if (controller.HasPermission(Permissions.AppAssetsUpdate))
            {
                response.AddPutLink("update", controller.Url<AssetsController>(x => nameof(x.PutAsset), values));
                response.AddPutLink("upload", controller.Url<AssetsController>(x => nameof(x.PutAssetContent), values));
            }

            if (controller.HasPermission(Permissions.AppAssetsDelete))
            {
                response.AddDeleteLink("delete", controller.Url<AssetsController>(x => nameof(x.DeleteAsset), values));
            }

            response.AddGetLink("content", controller.Url<AssetContentController>(x => nameof(x.GetAssetContent), new { id = response.Id, version = response.FileVersion }));

            if (!string.IsNullOrWhiteSpace(response.Slug))
            {
                response.AddGetLink("content/slug", controller.Url<AssetContentController>(x => nameof(x.GetAssetContentBySlug), new { app, idOrSlug = response.Slug, version = response.Version }));
            }

            return response;
        }
    }
}
