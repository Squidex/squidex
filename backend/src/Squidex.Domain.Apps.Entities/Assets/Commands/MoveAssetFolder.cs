// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.Commands
{
    public sealed class MoveAssetFolder : AssetFolderCommand
    {
        public DomainId ParentId { get; set; }

        public bool OptimizeValidation { get; set; }
    }
}
