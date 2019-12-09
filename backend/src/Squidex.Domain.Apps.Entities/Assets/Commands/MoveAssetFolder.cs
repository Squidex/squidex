// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Entities.Assets.Commands
{
    public sealed class MoveAssetFolder : AssetFolderCommand
    {
        public Guid ParentId { get; set; }
    }
}
