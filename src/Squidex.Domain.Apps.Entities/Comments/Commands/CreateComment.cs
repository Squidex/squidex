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
        public Guid CommentId { get; } = Guid.NewGuid();

        public string Text { get; set; }
    }
}
