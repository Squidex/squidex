// ==========================================================================
//  WebhookSchema.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Core.Webhooks
{
    public sealed class WebhookSchema
    {
        public Guid SchemaId { get; set; }

        public bool SendCreate { get; set; }

        public bool SendUpdate { get; set; }

        public bool SendDelete { get; set; }

        public bool SendPublish { get; set; }

        public bool SendUnpublish { get; set; }
    }
}
