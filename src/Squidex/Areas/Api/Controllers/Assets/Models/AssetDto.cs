﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using NodaTime;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Assets.Models
{
    public sealed class AssetDto
    {
        /// <summary>
        /// The id of the asset.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The file name.
        /// </summary>
        [Required]
        public string Name { get; set; }

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
        /// The size of the file in bytes.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// The version of the file.
        /// </summary>
        public long FileVersion { get; set; }

        /// <summary>
        /// Indicates whether the asset is a folder.
        /// </summary>
        public bool IsFolder { get; set; }

        /// <summary>
        /// Indicates where the asset is an image.
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

        public static AssetDto FromAsset(IAssetEntity asset)
        {
            return SimpleMapper.Map(asset, new AssetDto { FileType = asset.Name.FileType() });
        }
    }
}
