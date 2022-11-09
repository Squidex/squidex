// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Comments.Models;

public sealed class UpsertCommentDto
{
    /// <summary>
    /// The comment text.
    /// </summary>
    [LocalizedRequired]
    public string Text { get; set; }

    /// <summary>
    /// The url where the comment is created.
    /// </summary>
    public Uri? Url { get; set; }

    public CreateComment ToCreateCommand(DomainId commentsId)
    {
        return SimpleMapper.Map(this, new CreateComment
        {
            CommentsId = commentsId
        });
    }

    public UpdateComment ToUpdateComment(DomainId commentsId, DomainId commentId)
    {
        return SimpleMapper.Map(this, new UpdateComment
        {
            CommentsId = commentsId,
            CommentId = commentId
        });
    }
}
