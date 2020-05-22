// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Assets
{
    public abstract class AssetFolderEvent : AppEvent
    {
        public DomainId AssetFolderId { get; set; }
    }
}
