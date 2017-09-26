// ==========================================================================
//  AssetDeleted.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Assets
{
    [EventType(nameof(AssetDeleted))]
    public sealed class AssetDeleted : AssetEvent
    {
        public long DeletedSize { get; set; }
    }
}
