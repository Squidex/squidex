// ==========================================================================
//  WebhookEditCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Webhooks;

namespace Squidex.Domain.Apps.Write.Webhooks.Commands
{
    public abstract class WebhookEditCommand : WebhookAggregateCommand
    {
        public Uri Url { get; set; }

        public List<WebhookSchema> Schemas { get; set; } = new List<WebhookSchema>();
    }
}
