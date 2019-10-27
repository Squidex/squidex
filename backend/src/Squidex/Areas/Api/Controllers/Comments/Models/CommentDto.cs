// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using NodaTime;
using Squidex.Domain.Apps.Core.Comments;
using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Comments.Models
{
    public sealed class CommentDto
    {
        /// <summary>
        /// The id of the comment.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The time when the comment was created or updated last.
        /// </summary>
        [Required]
        public Instant Time { get; set; }

        /// <summary>
        /// The user who created or updated the comment.
        /// </summary>
        [Required]
        public RefToken User { get; set; }

        /// <summary>
        /// The text of the comment.
        /// </summary>
        [Required]
        public string Text { get; set; }

        public static CommentDto FromComment(Comment comment)
        {
            return SimpleMapper.Map(comment, new CommentDto());
        }

        public static CommentDto FromCommand(CreateComment command)
        {
            return SimpleMapper.Map(command, new CommentDto { Id = command.CommentId, User = command.Actor, Time = SystemClock.Instance.GetCurrentInstant() });
        }
    }
}
