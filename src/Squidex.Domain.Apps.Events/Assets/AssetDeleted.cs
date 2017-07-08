// ==========================================================================
//  AssetDeleted.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Assets
{
    [TypeName("AssetDeletedEvent")]
    public sealed class AssetDeleted : AssetEvent
    {
        public long DeletedSize { get; set; }
    }
}
