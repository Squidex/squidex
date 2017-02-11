// ==========================================================================
//  AppCreated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Events.Apps
{
    [TypeName("AppCreatedEvent")]
    public class AppCreated : AppEvent
    {
        public string Name { get; set; }
    }
}
