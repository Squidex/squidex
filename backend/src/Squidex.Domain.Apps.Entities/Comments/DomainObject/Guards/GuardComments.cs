// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Domain.Apps.Events.Comments;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Comments.DomainObject.Guards;

public static class GuardComments
{
    public static void CanCreate(CreateComment command)
    {
        Guard.NotNull(command);

        Validate.It(e =>
        {
            if (string.IsNullOrWhiteSpace(command.Text))
            {
                e(Not.Defined(nameof(command.Text)), nameof(command.Text));
            }
        });
    }

    public static void CanUpdate(UpdateComment command, string commentsId, List<Envelope<CommentsEvent>> events)
    {
        Guard.NotNull(command);

        var comment = FindComment(events, command.CommentId);

        if (!string.Equals(commentsId, command.Actor.Identifier, StringComparison.Ordinal) && !comment.Payload.Actor.Equals(command.Actor))
        {
            throw new DomainException(T.Get("comments.notUserComment"));
        }

        Validate.It(e =>
        {
            if (string.IsNullOrWhiteSpace(command.Text))
            {
                e(Not.Defined(nameof(command.Text)), nameof(command.Text));
            }
        });
    }

    public static void CanDelete(DeleteComment command, string commentsId, List<Envelope<CommentsEvent>> events)
    {
        Guard.NotNull(command);

        var comment = FindComment(events, command.CommentId);

        if (!string.Equals(commentsId, command.Actor.Identifier, StringComparison.Ordinal) && !comment.Payload.Actor.Equals(command.Actor))
        {
            throw new DomainException(T.Get("comments.notUserComment"));
        }
    }

    private static Envelope<CommentCreated> FindComment(List<Envelope<CommentsEvent>> events, DomainId commentId)
    {
        Envelope<CommentCreated>? result = null;

        foreach (var @event in events)
        {
            if (@event.Payload is CommentCreated created && created.CommentId == commentId)
            {
                result = @event.To<CommentCreated>();
            }
            else if (@event.Payload is CommentDeleted deleted && deleted.CommentId == commentId)
            {
                result = null;
            }
        }

        if (result == null)
        {
            throw new DomainObjectNotFoundException(commentId.ToString());
        }

        return result;
    }
}
