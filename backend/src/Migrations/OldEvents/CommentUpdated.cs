// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Migrations.OldEvents;

[EventType(nameof(CommentUpdated))]
public sealed class CommentUpdated : IEvent
{
    public string Text { get; set; }
}
