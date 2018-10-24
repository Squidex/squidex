// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Entities.Comments.Commands
{
    public sealed class UpdateComment : CommentsCommand
    {
        public Guid CommentId { get; set; }

        public string Text { get; set; }
    }
}
