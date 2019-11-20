// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Assets
{
    [EventType(nameof(AssetFolderCreated))]
    public sealed class AssetFolderCreated : AssetItemEvent
    {
        public Guid ParentId { get; set; }

        public string FolderName { get; set; }
    }
}
