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
using Squidex.Shared;
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
        [Required]
        public AssetDto[] Items { get; set; }

        public static AssetsDto FromAssets(IResultList<IEnrichedAssetEntity> assets, ApiController controller, string app)
        {
            var response = new AssetsDto
            {
                Total = assets.Total,
                Items = assets.Select(x => AssetDto.FromAsset(x, controller, app)).ToArray()
            };

            return CreateLinks(response, controller, app);
        }

        private static AssetsDto CreateLinks(AssetsDto response, ApiController controller, string app)
        {
            var values = new { app };

            response.AddSelfLink(controller.Url<AssetsController>(x => nameof(x.GetAssets), values));

            if (controller.HasPermission(Permissions.AppAssetsCreate))
            {
                response.AddPostLink("create", controller.Url<AssetsController>(x => nameof(x.PostAsset), values));
            }

            response.AddGetLink("tags", controller.Url<AssetsController>(x => nameof(x.GetTags), values));

            return response;
        }
    }
}
