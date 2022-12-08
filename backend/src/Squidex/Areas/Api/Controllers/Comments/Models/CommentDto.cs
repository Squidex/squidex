// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Comments;
using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Comments.Models;

public sealed class CommentDto
{
    /// <summary>
    /// The ID of the comment.
    /// </summary>
    public DomainId Id { get; set; }

    /// <summary>
    /// The time when the comment was created or updated last.
    /// </summary>
    [LocalizedRequired]
    public Instant Time { get; set; }

    /// <summary>
    /// The user who created or updated the comment.
    /// </summary>
    [LocalizedRequired]
    public RefToken User { get; set; }

    /// <summary>
    /// The text of the comment.
    /// </summary>
    [LocalizedRequired]
    public string Text { get; set; }

    /// <summary>
    /// The url where the comment is created.
    /// </summary>
    public Uri? Url { get; set; }

    public static CommentDto FromDomain(Comment comment)
    {
        var result = SimpleMapper.Map(comment, new CommentDto());

        return result;
    }

    public static CommentDto FromDomain(CreateComment command)
    {
        var time = SystemClock.Instance.GetCurrentInstant();

        return SimpleMapper.Map(command, new CommentDto { Id = command.CommentId, User = command.Actor, Time = time });
    }
}
