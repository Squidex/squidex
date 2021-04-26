// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Assets.Models
{
    public sealed class AssetsDto : Resource
    {
        /// <summary>
        /// The total number of assets.
        /// </summary>
        public long Total { get; set; }

        /// <summary>
        /// The assets.
        /// </summary>
        [LocalizedRequired]
        public AssetDto[] Items { get; set; }

        public static AssetsDto FromAssets(IResultList<IEnrichedAssetEntity> assets, Resources resources)
        {
            var response = new AssetsDto
            {
                Total = assets.Total,
                Items = assets.Select(x => AssetDto.FromAsset(x, resources)).ToArray()
            };

            return CreateLinks(response, resources);
        }

        private static AssetsDto CreateLinks(AssetsDto response, Resources resources)
        {
            var values = new { app = resources.App };

            response.AddSelfLink(resources.Url<AssetsController>(x => nameof(x.GetAssets), values));

            if (resources.CanCreateAsset)
            {
                response.AddPostLink("create", resources.Url<AssetsController>(x => nameof(x.PostAsset), values));
            }

            response.AddGetLink("tags", resources.Url<AssetsController>(x => nameof(x.GetTags), values));

            return response;
        }
    }
}
