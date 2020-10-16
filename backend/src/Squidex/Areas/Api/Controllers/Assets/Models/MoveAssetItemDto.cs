// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure;

namespace Squidex.Areas.Api.Controllers.Assets.Models
{
    public sealed class MoveAssetItemDto
    {
        /// <summary>
        /// The parent folder id.
        /// </summary>
        public DomainId ParentId { get; set; }

        public MoveAsset ToCommand(DomainId id)
        {
            return new MoveAsset { AssetId = id, ParentId = ParentId };
        }

        public MoveAssetFolder ToFolderCommand(DomainId id)
        {
            return new MoveAssetFolder { AssetFolderId = id, ParentId = ParentId };
        }
    }
}
