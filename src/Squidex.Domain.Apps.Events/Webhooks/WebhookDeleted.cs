// ==========================================================================
//  WebhookDeleted.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Webhooks
{
    [EventType(nameof(WebhookDeleted), 2)]
    public sealed class WebhookDeleted : WebhookEvent
    {
    }
}
