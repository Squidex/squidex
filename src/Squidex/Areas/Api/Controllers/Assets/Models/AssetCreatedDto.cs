// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;

namespace Squidex.Areas.Api.Controllers.Assets.Models
{
    public sealed class AssetCreatedDto
    {
        /// <summary>
        /// The id of the asset.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The file type.
        /// </summary>
        [Required]
        public string FileType { get; set; }

        /// <summary>
        /// The file name.
        /// </summary>
        [Required]
        public string FileName { get; set; }

        /// <summary>
        /// The mime type.
        /// </summary>
        [Required]
        public string MimeType { get; set; }

        /// <summary>
        /// The default tags.
        /// </summary>
        [Required]
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
        /// The version of the asset.
        /// </summary>
        public long Version { get; set; }

        public static AssetCreatedDto FromCommand(CreateAsset command, AssetCreatedResult result)
        {
            var response = new AssetCreatedDto
            {
                Id = command.AssetId,
                FileName = command.File.FileName,
                FileSize = command.File.FileSize,
                FileType = command.File.FileName.FileType(),
                FileVersion = result.Version,
                MimeType = command.File.MimeType,
                IsImage = command.ImageInfo != null,
                PixelWidth = command.ImageInfo?.PixelWidth,
                PixelHeight = command.ImageInfo?.PixelHeight,
                Tags = result.Tags,
                Version = result.Version
            };

            return response;
        }
    }
}
