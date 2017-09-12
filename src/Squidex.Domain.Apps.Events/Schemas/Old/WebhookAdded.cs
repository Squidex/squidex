// ==========================================================================
//  WebhookAdded.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Schemas.Old
{
    [EventType(nameof(WebhookAdded))]
    [Obsolete]
    public sealed class WebhookAdded : SchemaEvent, IMigratedEvent
    {
        public Guid Id { get; set; }

        public Uri Url { get; set; }

        public string SharedSecret { get; set; }

        public IEvent Migrate()
        {
            return new NoopEvent();
        }
    }
}
