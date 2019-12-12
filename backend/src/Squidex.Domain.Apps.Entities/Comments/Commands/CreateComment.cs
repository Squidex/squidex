// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Entities.Comments.Commands
{
    public sealed class CreateComment : CommentsCommand
    {
        public bool IsMention { get; set; }

        public string Text { get; set; }

        public string[]? Mentions { get; set; }

        public Uri? Url { get; set; }

        public CreateComment()
        {
            CommentId = Guid.NewGuid();
        }
    }
}
