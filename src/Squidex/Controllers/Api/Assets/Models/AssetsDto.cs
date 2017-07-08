// ==========================================================================
//  AssetsDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Controllers.Api.Assets.Models
{
    public sealed class AssetsDto
    {
        /// <summary>
        /// The total number of assets.
        /// </summary>
        public long Total { get; set; }

        /// <summary>
        /// The assets.
        /// </summary>
        public AssetDto[] Items { get; set; }
    }
}
