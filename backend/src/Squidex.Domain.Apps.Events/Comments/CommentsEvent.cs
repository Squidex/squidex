// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Events.Comments
{
    public abstract class CommentsEvent : AppEvent
    {
        public string CommentsId { get; set; }

        public Guid CommentId { get; set; }
    }
}
