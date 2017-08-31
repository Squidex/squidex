// ==========================================================================
//  WebhookDeleted.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Webhooks
{
    [TypeName("WebhookDeletedEventV2")]
    public sealed class WebhookDeleted : WebhookEvent
    {
    }
}
