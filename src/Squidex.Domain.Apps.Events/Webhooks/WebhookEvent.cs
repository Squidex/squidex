// ==========================================================================
//  WebhookEvent.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Events.Webhooks
{
    public abstract class WebhookEvent : AppEvent
    {
        public Guid WebhookId { get; set; }
    }
}
