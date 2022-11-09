// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.EventSourcing;

namespace Migrations.OldEvents;

[EventType(nameof(WebhookDeleted))]
[Obsolete("New Event introduced")]
public sealed class WebhookDeleted : SchemaEvent
{
    public Guid Id { get; set; }
}
