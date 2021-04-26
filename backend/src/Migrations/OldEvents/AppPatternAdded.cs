// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Migrations.OldEvents
{
    [EventType(nameof(AppPatternAdded))]
    [Obsolete("New Event introduced")]
    public sealed class AppPatternAdded : AppEvent
    {
        public DomainId PatternId { get; set; }

        public string Name { get; set; }

        public string Pattern { get; set; }

        public string? Message { get; set; }
    }
}
