// ==========================================================================
//  WebhookEditEvent.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Webhooks;

namespace Squidex.Domain.Apps.Events.Webhooks
{
    public abstract class WebhookEditEvent : WebhookEvent
    {
        public Uri Url { get; set; }

        public List<WebhookSchema> Schemas { get; set; }
    }
}
