// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Apps
{
    [EventType(nameof(AppPatternUpdated))]
    public sealed class AppPatternUpdated : AppEvent
    {
        public DomainId PatternId { get; set; }

        public string Name { get; set; }

        public string Pattern { get; set; }

        public string? Message { get; set; }
    }
}
