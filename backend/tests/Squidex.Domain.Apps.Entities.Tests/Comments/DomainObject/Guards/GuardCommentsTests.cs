// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Comments;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Comments.DomainObject.Guards;

public class GuardCommentsTests : IClassFixture<TranslationsFixture>
{
    private readonly string commentsId = DomainId.NewGuid().ToString();
    private readonly RefToken user1 = RefToken.User("1");
    private readonly RefToken user2 = RefToken.User("2");

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
        var commentId = DomainId.NewGuid();
        var command = new UpdateComment { CommentId = commentId, Actor = user1 };

        var events = new List<Envelope<CommentsEvent>>
        {
            Envelope.Create<CommentsEvent>(new CommentCreated { CommentId = commentId, Actor = user1 }).To<CommentsEvent>()
        };

        ValidationAssert.Throws(() => GuardComments.CanUpdate(command, commentsId, events),
            new ValidationError("Text is required.", "Text"));
    }

    [Fact]
    public void CanUpdate_should_throw_exception_if_comment_from_another_user()
    {
        var commentId = DomainId.NewGuid();
        var command = new UpdateComment { CommentId = commentId, Actor = user2, Text = "text2" };

        var events = new List<Envelope<CommentsEvent>>
        {
            Envelope.Create<CommentsEvent>(new CommentCreated { CommentId = commentId, Actor = user1 }).To<CommentsEvent>()
        };

        Assert.Throws<DomainException>(() => GuardComments.CanUpdate(command, commentsId, events));
    }

    [Fact]
    public void CanUpdate_should_throw_exception_if_comment_not_found()
    {
        var commentId = DomainId.NewGuid();
        var command = new UpdateComment { CommentId = commentId, Actor = user1 };

        var events = new List<Envelope<CommentsEvent>>();

        Assert.Throws<DomainObjectNotFoundException>(() => GuardComments.CanUpdate(command, commentsId, events));
    }

    [Fact]
    public void CanUpdate_should_throw_exception_if_comment_deleted_found()
    {
        var commentId = DomainId.NewGuid();
        var command = new UpdateComment { CommentId = commentId, Actor = user1 };

        var events = new List<Envelope<CommentsEvent>>
        {
            Envelope.Create<CommentsEvent>(new CommentCreated { CommentId = commentId, Actor = user1 }).To<CommentsEvent>(),
            Envelope.Create<CommentsEvent>(new CommentDeleted { CommentId = commentId }).To<CommentsEvent>()
        };

        Assert.Throws<DomainObjectNotFoundException>(() => GuardComments.CanUpdate(command, commentsId, events));
    }

    [Fact]
    public void CanUpdate_should_not_throw_exception_if_comment_is_own_notification()
    {
        var commentId = DomainId.NewGuid();
        var command = new UpdateComment { CommentId = commentId, Actor = user1, Text = "text2" };

        var events = new List<Envelope<CommentsEvent>>
        {
            Envelope.Create<CommentsEvent>(new CommentCreated { CommentId = commentId, Actor = user1 }).To<CommentsEvent>()
        };

        GuardComments.CanUpdate(command, user1.Identifier, events);
    }

    [Fact]
    public void CanUpdate_should_not_throw_exception_if_comment_from_same_user()
    {
        var commentId = DomainId.NewGuid();
        var command = new UpdateComment { CommentId = commentId, Actor = user1, Text = "text2" };

        var events = new List<Envelope<CommentsEvent>>
        {
            Envelope.Create<CommentsEvent>(new CommentCreated { CommentId = commentId, Actor = user1 }).To<CommentsEvent>()
        };

        GuardComments.CanUpdate(command, commentsId, events);
    }

    [Fact]
    public void CanDelete_should_throw_exception_if_comment_from_another_user()
    {
        var commentId = DomainId.NewGuid();
        var command = new DeleteComment { CommentId = commentId, Actor = user2 };

        var events = new List<Envelope<CommentsEvent>>
        {
            Envelope.Create<CommentsEvent>(new CommentCreated { CommentId = commentId, Actor = user1 }).To<CommentsEvent>()
        };

        Assert.Throws<DomainException>(() => GuardComments.CanDelete(command, commentsId, events));
    }

    [Fact]
    public void CanDelete_should_throw_exception_if_comment_not_found()
    {
        var commentId = DomainId.NewGuid();
        var command = new DeleteComment { CommentId = commentId, Actor = user1 };

        var events = new List<Envelope<CommentsEvent>>();

        Assert.Throws<DomainObjectNotFoundException>(() => GuardComments.CanDelete(command, commentsId, events));
    }

    [Fact]
    public void CanDelete_should_throw_exception_if_comment_deleted()
    {
        var commentId = DomainId.NewGuid();
        var command = new DeleteComment { CommentId = commentId, Actor = user1 };

        var events = new List<Envelope<CommentsEvent>>
        {
            Envelope.Create<CommentsEvent>(new CommentCreated { CommentId = commentId, Actor = user1 }),
            Envelope.Create<CommentsEvent>(new CommentDeleted { CommentId = commentId })
        };

        Assert.Throws<DomainObjectNotFoundException>(() => GuardComments.CanDelete(command, commentsId, events));
    }

    [Fact]
    public void CanDelete_should_not_throw_exception_if_comment_is_own_notification()
    {
        var commentId = DomainId.NewGuid();
        var command = new DeleteComment { CommentId = commentId, Actor = user1 };

        var events = new List<Envelope<CommentsEvent>>
        {
            Envelope.Create<CommentsEvent>(new CommentCreated { CommentId = commentId, Actor = user1 }).To<CommentsEvent>()
        };

        GuardComments.CanDelete(command, user1.Identifier, events);
    }

    [Fact]
    public void CanDelete_should_not_throw_exception_if_comment_from_same_user()
    {
        var commentId = DomainId.NewGuid();
        var command = new DeleteComment { CommentId = commentId, Actor = user1 };

        var events = new List<Envelope<CommentsEvent>>
        {
            Envelope.Create<CommentsEvent>(new CommentCreated { CommentId = commentId, Actor = user1 }).To<CommentsEvent>()
        };

        GuardComments.CanDelete(command, commentsId, events);
    }
}
