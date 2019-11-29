﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Assets.Models
{
    public sealed class AnnotateAssetDto
    {
        /// <summary>
        /// The new name of the asset.
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// The new slug of the asset.
        /// </summary>
        public string? Slug { get; set; }

        /// <summary>
        /// The new asset tags.
        /// </summary>
        public HashSet<string>? Tags { get; set; }

        public AnnotateAsset ToCommand(Guid id)
        {
            return SimpleMapper.Map(this, new AnnotateAsset { AssetId = id });
        }
    }
}
