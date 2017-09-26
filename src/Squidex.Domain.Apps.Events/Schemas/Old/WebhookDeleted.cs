// ==========================================================================
//  WebhookDeleted.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Schemas.Old
{
    [EventType(nameof(WebhookDeleted))]
    [Obsolete]
    public sealed class WebhookDeleted : SchemaEvent
    {
        public Guid Id { get; set; }
    }
}
