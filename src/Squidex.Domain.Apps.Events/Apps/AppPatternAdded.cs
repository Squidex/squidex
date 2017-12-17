// ==========================================================================
//  AppPatternAdded.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Apps
{
    [EventType(nameof(AppPatternAdded))]
    public sealed class AppPatternAdded : AppEvent
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Pattern { get; set; }

        public string Message { get; set; }
    }
}
