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
using Squidex.Pipeline;

namespace Squidex.Areas.Api.Controllers.Assets.Models
{
    public sealed class AssetsDto : IGenerateEtag
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

        public string GenerateETag()
        {
            return string.Join(";", Items?.Select(x => x.GenerateETag()) ?? Enumerable.Empty<string>()).Sha256Base64();
        }

        public static AssetsDto FromAssets(IResultList<IAssetEntity> assets)
        {
            return new AssetsDto { Total = assets.Total, Items = assets.Select(AssetDto.FromAsset).ToArray() };
        }
    }
}
