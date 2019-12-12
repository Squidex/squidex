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
    [EventType(nameof(CommentCreated))]
    public sealed class CommentCreated : CommentsEvent
    {
        public string Text { get; set; }

        public string[]? Mentions { get; set; }

        public Uri? Url { get; set; }
    }
}
