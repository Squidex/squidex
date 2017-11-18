// ==========================================================================
//  AssetReplacedDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Write.Assets;
using Squidex.Domain.Apps.Write.Assets.Commands;

namespace Squidex.Controllers.Api.Assets.Models
{
    public sealed class AssetReplacedDto
    {
        /// <summary>
        /// The mime type.
        /// </summary>
        [Required]
        public string MimeType { get; set; }

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

        public static AssetReplacedDto Create(UpdateAsset command, AssetSavedResult result)
        {
            var response = new AssetReplacedDto
            {
                FileSize = command.File.FileSize,
                FileVersion = result.FileVersion,
                MimeType = command.File.MimeType,
                IsImage = command.ImageInfo != null,
                PixelWidth = command.ImageInfo?.PixelWidth,
                PixelHeight = command.ImageInfo?.PixelHeight,
                Version = result.Version
            };

            return response;
        }
    }
}
