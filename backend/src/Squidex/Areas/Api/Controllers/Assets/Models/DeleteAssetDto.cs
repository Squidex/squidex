// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Assets.Models
{
    public sealed class DeleteAssetDto
    {
        /// <summary>
        /// True to check referrers of this asset.
        /// </summary>
        [FromQuery]
        public bool CheckReferrers { get; set; }

        /// <summary>
        /// True to delete the asset permanently.
        /// </summary>
        [FromQuery]
        public bool Permanent { get; set; }

        public DeleteAsset ToCommand(DomainId id)
        {
            return SimpleMapper.Map(this, new DeleteAsset { AssetId = id });
        }
    }
}
