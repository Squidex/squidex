// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Comments
{
    [EventType(nameof(CommentUpdated))]
    public sealed class CommentUpdated : CommentsEvent
    {
        public Guid CommentId { get; set; }

        public string Text { get; set; }
    }
}
