// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Areas.Api.Controllers.Assets.Models
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
