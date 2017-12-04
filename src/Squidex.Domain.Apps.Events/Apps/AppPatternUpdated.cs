// ==========================================================================
//  AppPatternUpdated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Apps
{
    [EventType(nameof(AppPatternUpdated))]
    public sealed class AppPatternUpdated : AppEvent
    {
        public string OriginalName { get; set; }

        public string Name { get; set; }

        public string Pattern { get; set; }

        public string DefaultMessage { get; set; }
    }
}
