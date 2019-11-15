// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Infrastructure;

namespace Squidex.Areas.Api.Controllers.Assets.Models
{
    public sealed class AssetQuery
    {
        /// <summary>
        /// The optional version of the asset.
        /// </summary>
        [FromQuery(Name = "version")]
        public long Version { get; set; } = EtagVersion.Any;

        /// <summary>
        /// The cache duration in seconds.
        /// </summary>
        [FromQuery(Name = "cache")]
        public long CacheDuration { get; set; }

        /// <summary>
        /// Set it to 0 to prevent download.
        /// </summary>
        [FromQuery(Name = "download")]
        public int Download { get; set; } = 1;

        /// <summary>
        /// The target width of the asset, if it is an image.
        /// </summary>
        [FromQuery(Name = "width")]
        public int? Width { get; set; }

        /// <summary>
        /// The target height of the asset, if it is an image.
        /// </summary>
        [FromQuery(Name = "height")]
        public int? Height { get; set; }

        /// <summary>
        /// Optional image quality, it is is an jpeg image.
        /// </summary>
        [FromQuery(Name = "quality")]
        public int? Quality { get; set; }

        /// <summary>
        /// The resize mode when the width and height is defined.
        /// </summary>
        [FromQuery(Name = "mode")]
        public string? Mode { get; set; }

        public bool ShouldResize()
        {
            return Width.HasValue || Height.HasValue || Quality.HasValue;
        }
    }
}
