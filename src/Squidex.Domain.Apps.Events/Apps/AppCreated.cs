// ==========================================================================
//  AppCreated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Apps
{
    [TypeName("AppCreatedEvent")]
    public sealed class AppCreated : AppEvent
    {
        public string Name { get; set; }
    }
}
