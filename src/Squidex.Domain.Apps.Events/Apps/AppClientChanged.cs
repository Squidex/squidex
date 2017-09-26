// ==========================================================================
//  AppClientChanged.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Apps
{
    [EventType(nameof(AppClientChanged))]
    public sealed class AppClientChanged : AppEvent
    {
        public string Id { get; set; }

        public bool IsReader { get; set; }
    }
}
