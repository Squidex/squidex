// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Apps
{
    [EventType(nameof(AppPatternAdded))]
    public sealed class AppPatternAdded : AppEvent
    {
        public Guid PatternId { get; set; }

        public string Name { get; set; }

        public string Pattern { get; set; }

        public string Message { get; set; }
    }
}
