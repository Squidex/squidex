// ==========================================================================
//  AppCreated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Apps
{
    [EventType(nameof(AppCreated))]
    public sealed class AppCreated : AppEvent
    {
        public string Name { get; set; }
    }
}
