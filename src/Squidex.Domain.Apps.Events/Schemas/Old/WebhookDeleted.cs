// ==========================================================================
//  WebhookDeleted.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Schemas.Old
{
    [TypeName("WebhookDeletedEvent")]
    [Obsolete]
    public sealed class WebhookDeleted : SchemaEvent
    {
        public Guid Id { get; set; }
    }
}
