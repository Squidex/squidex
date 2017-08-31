// ==========================================================================
//  WebhookAdded.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Schemas.Old
{
    [TypeName("WebhookAddedEvent")]
    [Obsolete]
    public sealed class WebhookAdded : SchemaEvent
    {
        public Guid Id { get; set; }

        public Uri Url { get; set; }

        public string SharedSecret { get; set; }
    }
}
