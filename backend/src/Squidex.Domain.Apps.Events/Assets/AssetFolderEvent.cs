// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Events.Assets
{
    public abstract class AssetFolderEvent : AppEvent
    {
        public Guid AssetFolderId { get; set; }
    }
}
