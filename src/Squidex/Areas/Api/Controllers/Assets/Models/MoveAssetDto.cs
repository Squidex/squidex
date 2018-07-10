// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Assets.Models
{
    public sealed class MoveAssetDto
    {
        /// <summary>
        /// The new folder id.
        /// </summary>
        public Guid FolderId { get; set; }

        public MoveAsset ToCommand(Guid id)
        {
            return SimpleMapper.Map(this, new MoveAsset { AssetId = id });
        }
    }
}
