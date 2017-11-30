// ==========================================================================
//  AppPatternAdded.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Apps
{
    [EventType(nameof(AppPatternAdded))]
    public sealed class AppPatternAdded : AppEvent
    {
        public string Name { get; set; }

        public string Pattern { get; set; }

        public string DefaultMessage { get; set; }
    }
}
