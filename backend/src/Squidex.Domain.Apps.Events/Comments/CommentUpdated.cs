// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Comments;

[EventType(nameof(CommentUpdated))]
public sealed class CommentUpdated : CommentsEvent
{
    public string Text { get; set; }
}
