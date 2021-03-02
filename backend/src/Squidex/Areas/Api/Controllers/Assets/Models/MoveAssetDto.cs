// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Assets.Models
{
    public sealed class MoveAssetDto
    {
        /// <summary>
        /// The parent folder id.
        /// </summary>
        public DomainId ParentId { get; set; }

        /// <summary>
        /// The optional path to the folder.
        /// </summary>
        public string? ParentPath { get; set; }

        public MoveAsset ToCommand(DomainId id)
        {
            return SimpleMapper.Map(this, new MoveAsset { AssetId = id });
        }
    }
}
