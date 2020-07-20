// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Entities.Comments;
using Squidex.Infrastructure;

namespace Squidex.Areas.Api.Controllers.Comments.Models
{
    public sealed class CommentsDto
    {
        /// <summary>
        /// The created comments including the updates.
        /// </summary>
        public CommentDto[]? CreatedComments { get; set; }

        /// <summary>
        /// The updates comments since the last version.
        /// </summary>
        public CommentDto[]? UpdatedComments { get; set; }

        /// <summary>
        /// The deleted comments since the last version.
        /// </summary>
        public List<DomainId>? DeletedComments { get; set; }

        /// <summary>
        /// The current version.
        /// </summary>
        public long Version { get; set; }

        public static CommentsDto FromResult(CommentsResult result)
        {
            return new CommentsDto
            {
                CreatedComments = result.CreatedComments.Select(CommentDto.FromComment).ToArray(),
                UpdatedComments = result.UpdatedComments.Select(CommentDto.FromComment).ToArray(),
                DeletedComments = result.DeletedComments,
                Version = result.Version
            };
        }
    }
}
