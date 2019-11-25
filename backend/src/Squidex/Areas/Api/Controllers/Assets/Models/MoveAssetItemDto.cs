// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Entities.Assets.Commands;

namespace Squidex.Areas.Api.Controllers.Assets.Models
{
    public sealed class MoveAssetItemDto
    {
        /// <summary>
        /// The parent folder id.
        /// </summary>
        public Guid ParentId { get; set; }

        public MoveAsset ToCommand(Guid id)
        {
            return new MoveAsset { AssetId = id, ParentId = ParentId };
        }

        public MoveAssetFolder ToFolderCommand(Guid id)
        {
            return new MoveAssetFolder { AssetFolderId = id, ParentId = ParentId };
        }
    }
}
