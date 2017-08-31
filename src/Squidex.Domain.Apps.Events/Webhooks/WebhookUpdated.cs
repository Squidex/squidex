// ==========================================================================
//  WebhookUpdated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Webhooks
{
    [TypeName("WebhookUpdatedEvent")]
    public sealed class WebhookUpdated : WebhookEditEvent
    {
    }
}
