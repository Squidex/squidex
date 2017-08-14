// ==========================================================================
//  WebhookJob.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using NodaTime;

namespace Squidex.Domain.Apps.Read.Webhooks
{
    public sealed class WebhookJob
    {
        public Guid Id { get; set; }

        public Guid AppId { get; set; }

        public Guid WebhookId { get; set; }

        public Uri RequestUrl { get; set; }

        public string RequestBody { get; set; }

        public string RequestSignature { get; set; }

        public string EventName { get; set; }

        public Instant Expires { get; set; }
    }
}
