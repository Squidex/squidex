// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Domain.Apps.Events.Comments;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Comments.Guards
{
    public static class GuardComments
    {
        public static void CanCreate(CreateComment command)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot create comment.", e =>
            {
                if (string.IsNullOrWhiteSpace(command.Text))
                {
                   e(Not.Defined("Text"), nameof(command.Text));
                }
            });
        }

        public static void CanUpdate(List<Envelope<CommentsEvent>> events, UpdateComment command)
        {
            Guard.NotNull(command, nameof(command));

            var comment = FindComment(events, command.CommentId);

            if (!comment.Payload.Actor.Equals(command.Actor))
            {
                throw new DomainException("Comment is created by another actor.");
            }

            Validate.It(() => "Cannot update comment.", e =>
            {
                if (string.IsNullOrWhiteSpace(command.Text))
                {
                   e(Not.Defined("Text"), nameof(command.Text));
                }
            });
        }

        public static void CanDelete(List<Envelope<CommentsEvent>> events, DeleteComment command)
        {
            Guard.NotNull(command, nameof(command));

            var comment = FindComment(events, command.CommentId);

            if (!comment.Payload.Actor.Equals(command.Actor))
            {
                throw new DomainException("Comment is created by another actor.");
            }
        }

        private static Envelope<CommentCreated> FindComment(List<Envelope<CommentsEvent>> events, Guid commentId)
        {
            Envelope<CommentCreated> result = null;

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
                throw new DomainObjectNotFoundException(commentId.ToString(), "Comments", typeof(CommentsGrain));
            }

            return result;
        }
    }
}
