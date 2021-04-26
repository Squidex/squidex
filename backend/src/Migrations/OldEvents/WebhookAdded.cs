// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.EventSourcing;

namespace Migrations.OldEvents
{
    [EventType(nameof(WebhookAdded))]
    [Obsolete("New Event introduced")]
    public sealed class WebhookAdded : SchemaEvent
    {
        public Guid Id { get; set; }

        public Uri Url { get; set; }

        public string SharedSecret { get; set; }
    }
}
