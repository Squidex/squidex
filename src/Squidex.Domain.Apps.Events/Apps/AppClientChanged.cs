// ==========================================================================
//  AppClientChanged.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Apps
{
    [TypeName("AppClientChangedEvent")]
    public sealed class AppClientChanged : AppEvent
    {
        public string Id { get; set; }

        public bool IsReader { get; set; }
    }
}
