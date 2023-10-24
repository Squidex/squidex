// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Migrations.OldEvents;

[EventType(nameof(CommentDeleted))]
public sealed class CommentDeleted : IEvent
{
}
