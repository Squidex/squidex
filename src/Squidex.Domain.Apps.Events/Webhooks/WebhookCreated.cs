// ==========================================================================
//  WebhookCreated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Webhooks
{
    [EventType(nameof(WebhookCreated))]
    public sealed class WebhookCreated : WebhookEditEvent
    {
        public string SharedSecret { get; set; }
    }
}
