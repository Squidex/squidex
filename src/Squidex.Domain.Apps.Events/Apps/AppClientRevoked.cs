// ==========================================================================
//  AppClientRevoked.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Apps
{
    [EventType(nameof(AppClientRevoked))]
    public sealed class AppClientRevoked : AppEvent
    {
        public string Id { get; set; }
    }
}
