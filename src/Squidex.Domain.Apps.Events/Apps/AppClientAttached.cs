// ==========================================================================
//  AppClientAttached.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Apps
{
    [EventType(nameof(AppClientAttached))]
    public sealed class AppClientAttached : AppEvent
    {
        public string Id { get; set; }

        public string Secret { get; set; }
    }
}
