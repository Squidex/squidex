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
    [EventType(nameof(AppPatternDeleted))]
    [Obsolete("New Event introduced")]
    public sealed class AppPatternDeleted : AppEvent
    {
        public DomainId PatternId { get; set; }
    }
}
