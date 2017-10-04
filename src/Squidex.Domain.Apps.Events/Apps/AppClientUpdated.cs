// ==========================================================================
//  AppClientChanged.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Apps
{
    [EventType(nameof(AppClientUpdated))]
    public sealed class AppClientUpdated : AppEvent
    {
        public string Id { get; set; }

        public AppClientPermission Permission { get; set; }
    }
}
