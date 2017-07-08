// ==========================================================================
//  AppClientAttached.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Events.Apps
{
    [TypeName("AppClientAttachedEvent")]
    public sealed class AppClientAttached : AppEvent
    {
        public string Id { get; set; }

        public string Secret { get; set; }
    }
}
