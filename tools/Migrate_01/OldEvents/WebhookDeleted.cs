// ==========================================================================
//  WebhookDeleted.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.EventSourcing;

namespace Migrate_01.OldEvents
{
    [EventType(nameof(WebhookDeleted))]
    [Obsolete]
    public sealed class WebhookDeleted : SchemaEvent
    {
        public Guid Id { get; set; }
    }
}
