﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Assets.Commands
{
    public sealed class RenameAssetFolder : AssetItemCommand
    {
        public string Name { get; set; }
    }
}
