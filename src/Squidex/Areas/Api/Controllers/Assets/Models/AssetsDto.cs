// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;

namespace Squidex.Areas.Api.Controllers.Assets.Models
{
    public sealed class AssetsDto
    {
        /// <summary>
        /// The assets.
        /// </summary>
        [Required]
        public AssetDto[] Items { get; set; }

        /// <summary>
        /// The total number of assets.
        /// </summary>
        public long Total { get; set; }

        public static AssetsDto FromAssets(IResultList<IAssetEntity> assets)
        {
            return new AssetsDto { Total = assets.Total, Items = assets.Select(AssetDto.FromAsset).ToArray() };
        }
    }
}
