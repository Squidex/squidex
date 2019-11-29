// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Assets
{
    [EventType(nameof(AssetFolderMoved))]
    public sealed class AssetFolderMoved : AssetFolderEvent
    {
        public Guid ParentId { get; set; }
    }
}
