// ==========================================================================
//  AssetCreated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Events.Assets
{
    [TypeName("AssetCreatedEvent")]
    public class AssetCreated : AssetEvent
    {
        public string Name { get; set; }
    }
}
