﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Assets.Models
{
    public sealed class AssetContentQueryDto
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
        public int Download { get; set; } = 0;

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
        public ResizeMode? Mode { get; set; }

        /// <summary>
        /// Override the y focus point.
        /// </summary>
        [FromQuery(Name = "focusX")]
        public float? FocusX { get; set; }

        /// <summary>
        /// Override the x focus point.
        /// </summary>
        [FromQuery(Name = "focusY")]
        public float? FocusY { get; set; }

        /// <summary>
        /// True to ignore the asset focus point if any.
        /// </summary>
        [FromQuery(Name = "nofocus")]
        public bool IgnoreFocus { get; set; }

        /// <summary>
        /// True to force a new resize even if it already stored.
        /// </summary>
        [FromQuery(Name = "force")]
        public bool ForceResize { get; set; }

        public ResizeOptions ToResizeOptions(IAssetEntity asset)
        {
            Guard.NotNull(asset);

            var result = SimpleMapper.Map(this, new ResizeOptions());

            var (x, y) = GetFocusPoint(asset);

            result.FocusX = x;
            result.FocusY = y;

            return result;
        }

        private (float?, float?) GetFocusPoint(IAssetEntity asset)
        {
            if (!IgnoreFocus)
            {
                if (FocusX.HasValue && FocusY.HasValue)
                {
                    return (FocusX.Value, FocusY.Value);
                }

                var focusX = asset.Metadata.GetFocusX();
                var focusY = asset.Metadata.GetFocusY();

                if (focusX.HasValue && focusY.HasValue)
                {
                    return (focusX.Value, focusY.Value);
                }
            }

            return (null, null);
        }
    }
}
