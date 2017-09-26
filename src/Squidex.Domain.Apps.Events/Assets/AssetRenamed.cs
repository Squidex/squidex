// ==========================================================================
//  AssetRenamed.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Assets
{
    [EventType(nameof(AssetRenamed))]
    public sealed class AssetRenamed : AssetEvent
    {
        public string FileName { get; set; }
    }
}
