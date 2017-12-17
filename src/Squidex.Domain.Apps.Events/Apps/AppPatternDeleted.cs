// ==========================================================================
//  AppPatternDeleted.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Apps
{
    [EventType(nameof(AppPatternDeleted))]
    public sealed class AppPatternDeleted : AppEvent
    {
        public Guid PatternId { get; set; }

        public string Name { get; set; }
    }
}
