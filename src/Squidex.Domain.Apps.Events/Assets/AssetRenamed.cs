// ==========================================================================
//  AssetRenamed.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Assets
{
    [TypeName("AssetRenamedEvent")]
    public sealed class AssetRenamed : AssetEvent
    {
        public string FileName { get; set; }
    }
}
