// ==========================================================================
//  AssetDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using NodaTime;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Write.Assets.Commands;

namespace Squidex.Controllers.Api.Assets.Models
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
        public string FileName { get; set; }

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

        public static AssetDto Create(CreateAsset command, EntityCreatedResult<Guid> result)
        {
            var now = SystemClock.Instance.GetCurrentInstant();

            var response = new AssetDto
            {
                Id = result.IdOrValue,
                Version = result.Version,
                Created = now,
                CreatedBy = command.Actor,
                LastModified = now,
                LastModifiedBy = command.Actor,
                FileName = command.File.FileName,
                FileSize = command.File.FileSize,
                MimeType = command.File.MimeType,
                IsImage = command.ImageInfo != null,
                PixelWidth = command.ImageInfo?.PixelWidth,
                PixelHeight = command.ImageInfo?.PixelHeight
            };

            return response;
        }
    }
}
