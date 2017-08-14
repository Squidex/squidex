// ==========================================================================
//  WebhookCreated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Webhooks
{
    [TypeName("WebhookCreatedEvent")]
    public sealed class WebhookCreated : WebhookEditEvent
    {
        public string SharedSecret { get; set; }
    }
}
