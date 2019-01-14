// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Comments;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Comments.Guards
{
    public class GuardCommentsTests
    {
        private readonly RefToken user1 = new RefToken(RefTokenType.Subject, "1");
        private readonly RefToken user2 = new RefToken(RefTokenType.Subject, "2");

        [Fact]
        public void CanCreate_should_throw_exception_if_text_not_defined()
        {
            var command = new CreateComment();

            ValidationAssert.Throws(() => GuardComments.CanCreate(command),
                new ValidationError("Text is required.", "Text"));
        }

        [Fact]
        public void CanCreate_should_not_throw_exception_if_text_defined()
        {
            var command = new CreateComment { Text = "text" };

            GuardComments.CanCreate(command);
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_text_not_defined()
        {
            var commentId = Guid.NewGuid();
            var command = new UpdateComment { CommentId = commentId, Actor = user1 };

            var events = new List<Envelope<CommentsEvent>>
            {
                Envelope.Create<CommentsEvent>(new CommentCreated { CommentId = commentId, Actor = user1 }).To<CommentsEvent>()
            };

            ValidationAssert.Throws(() => GuardComments.CanUpdate(events, command),
                new ValidationError("Text is required.", "Text"));
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_comment_from_another_user()
        {
            var commentId = Guid.NewGuid();
            var command = new UpdateComment { CommentId = commentId, Actor = user2, Text = "text2" };

            var events = new List<Envelope<CommentsEvent>>
            {
                Envelope.Create<CommentsEvent>(new CommentCreated { CommentId = commentId, Actor = user1 }).To<CommentsEvent>()
            };

            Assert.Throws<DomainException>(() => GuardComments.CanUpdate(events, command));
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_comment_not_found()
        {
            var commentId = Guid.NewGuid();
            var command = new UpdateComment { CommentId = commentId, Actor = user1 };

            var events = new List<Envelope<CommentsEvent>>();

            Assert.Throws<DomainObjectNotFoundException>(() => GuardComments.CanUpdate(events, command));
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_comment_deleted_found()
        {
            var commentId = Guid.NewGuid();
            var command = new UpdateComment { CommentId = commentId, Actor = user1 };

            var events = new List<Envelope<CommentsEvent>>
            {
                Envelope.Create<CommentsEvent>(new CommentCreated { CommentId = commentId, Actor = user1 }).To<CommentsEvent>(),
                Envelope.Create<CommentsEvent>(new CommentDeleted { CommentId = commentId }).To<CommentsEvent>()
            };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardComments.CanUpdate(events, command));
        }

        [Fact]
        public void CanUpdate_should_not_throw_exception_if_comment_from_same_user()
        {
            var commentId = Guid.NewGuid();
            var command = new UpdateComment { CommentId = commentId, Actor = user1, Text = "text2" };

            var events = new List<Envelope<CommentsEvent>>
            {
                Envelope.Create<CommentsEvent>(new CommentCreated { CommentId = commentId, Actor = user1 }).To<CommentsEvent>()
            };

            GuardComments.CanUpdate(events, command);
        }

        [Fact]
        public void CanDelete_should_throw_exception_if_comment_from_another_user()
        {
            var commentId = Guid.NewGuid();
            var command = new DeleteComment { CommentId = commentId, Actor = user2 };

            var events = new List<Envelope<CommentsEvent>>
            {
                Envelope.Create<CommentsEvent>(new CommentCreated { CommentId = commentId, Actor = user1 }).To<CommentsEvent>()
            };

            Assert.Throws<DomainException>(() => GuardComments.CanDelete(events, command));
        }

        [Fact]
        public void CanDelete_should_throw_exception_if_comment_not_found()
        {
            var commentId = Guid.NewGuid();
            var command = new DeleteComment { CommentId = commentId, Actor = user1 };

            var events = new List<Envelope<CommentsEvent>>();

            Assert.Throws<DomainObjectNotFoundException>(() => GuardComments.CanDelete(events, command));
        }

        [Fact]
        public void CanDelete_should_throw_exception_if_comment_deleted()
        {
            var commentId = Guid.NewGuid();
            var command = new DeleteComment { CommentId = commentId, Actor = user1 };

            var events = new List<Envelope<CommentsEvent>>
            {
                Envelope.Create<CommentsEvent>(new CommentCreated { CommentId = commentId, Actor = user1 }),
                Envelope.Create<CommentsEvent>(new CommentDeleted { CommentId = commentId })
            };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardComments.CanDelete(events, command));
        }

        [Fact]
        public void CanDelete_should_not_throw_exception_if_comment_from_same_user()
        {
            var commentId = Guid.NewGuid();
            var command = new DeleteComment { CommentId = commentId, Actor = user1 };

            var events = new List<Envelope<CommentsEvent>>
            {
                Envelope.Create<CommentsEvent>(new CommentCreated { CommentId = commentId, Actor = user1 }).To<CommentsEvent>()
            };

            GuardComments.CanDelete(events, command);
        }
    }
}
