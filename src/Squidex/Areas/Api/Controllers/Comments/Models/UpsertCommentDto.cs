// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Comments.Models
{
    public sealed class UpsertCommentDto
    {
        /// <summary>
        /// The comment text.
        /// </summary>
        [Required]
        public string Text { get; set; }

        public CreateComment ToCreateCommand(Guid commentsId)
        {
            return SimpleMapper.Map(this, new CreateComment { CommentsId = commentsId });
        }

        public UpdateComment ToUpdateComment(Guid commentsId, Guid commentId)
        {
            return SimpleMapper.Map(this, new UpdateComment { CommentsId = commentsId, CommentId = commentId });
        }
    }
}
