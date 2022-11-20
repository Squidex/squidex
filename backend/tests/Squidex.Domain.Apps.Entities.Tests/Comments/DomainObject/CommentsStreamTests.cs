// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Comments;
using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Comments;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Comments.DomainObject;

public class CommentsStreamTests
{
    private readonly IEventFormatter eventFormatter = A.Fake<IEventFormatter>();
    private readonly IEventStore eventStore = A.Fake<IEventStore>();
    private readonly DomainId commentsId = DomainId.NewGuid();
    private readonly DomainId commentId = DomainId.NewGuid();
    private readonly RefToken actor = RefToken.User("me");
    private readonly CommentsStream sut;

    public IEnumerable<Envelope<IEvent>> LastEvents { get; private set; } = Enumerable.Empty<Envelope<IEvent>>();

    public CommentsStreamTests()
    {
        A.CallTo(() => eventStore.AppendAsync(A<Guid>._, A<string>._, A<long>._, A<ICollection<EventData>>._, default))
            .Invokes(x => LastEvents = sut!.GetUncommittedEvents().Select(x => x.To<IEvent>()).ToList());

        sut = new CommentsStream(commentsId, eventFormatter, eventStore);
    }

    [Fact]
    public async Task Create_should_create_events()
    {
        var command = new CreateComment { Text = "text1", Url = new Uri("http://uri") };

        var actual = await sut.ExecuteAsync(CreateCommentsCommand(command), default);

        actual.ShouldBeEquivalent(CommandResult.Empty(commentsId, 0, EtagVersion.Empty));

        sut.GetComments(0).Should().BeEquivalentTo(new CommentsResult
        {
            Version = 0
        });

        sut.GetComments(-1).Should().BeEquivalentTo(new CommentsResult
        {
            CreatedComments = new List<Comment>
            {
                new Comment(command.CommentId, GetTime(), command.Actor, "text1", command.Url)
            },
            Version = 0
        });

        LastEvents
            .ShouldHaveSameEvents(
                CreateCommentsEvent(new CommentCreated { Text = command.Text, Url = command.Url })
            );
    }

    [Fact]
    public async Task Update_should_create_events()
    {
        await ExecuteCreateAsync();

        var updateCommand = new UpdateComment { Text = "text2" };

        var actual = await sut.ExecuteAsync(CreateCommentsCommand(updateCommand), default);

        actual.ShouldBeEquivalent(CommandResult.Empty(commentsId, 1, 0));

        sut.GetComments(-1).Should().BeEquivalentTo(new CommentsResult
        {
            CreatedComments = new List<Comment>
            {
                new Comment(commentId, GetTime(), updateCommand.Actor, "text2")
            },
            Version = 1
        });

        sut.GetComments(0).Should().BeEquivalentTo(new CommentsResult
        {
            UpdatedComments = new List<Comment>
            {
                new Comment(commentId, GetTime(), updateCommand.Actor, "text2")
            },
            Version = 1
        });

        LastEvents
            .ShouldHaveSameEvents(
                CreateCommentsEvent(new CommentUpdated { Text = updateCommand.Text })
            );
    }

    [Fact]
    public async Task Delete_should_create_events()
    {
        await ExecuteCreateAsync();
        await ExecuteUpdateAsync();

        var deleteCommand = new DeleteComment();

        var actual = await sut.ExecuteAsync(CreateCommentsCommand(deleteCommand), default);

        actual.ShouldBeEquivalent(CommandResult.Empty(commentsId, 2, 1));

        sut.GetComments(-1).Should().BeEquivalentTo(new CommentsResult
        {
            Version = 2
        });

        sut.GetComments(0).Should().BeEquivalentTo(new CommentsResult
        {
            DeletedComments = new List<DomainId>
            {
                commentId
            },
            Version = 2
        });

        sut.GetComments(1).Should().BeEquivalentTo(new CommentsResult
        {
            DeletedComments = new List<DomainId>
            {
                commentId
            },
            Version = 2
        });

        LastEvents
            .ShouldHaveSameEvents(
                CreateCommentsEvent(new CommentDeleted())
            );
    }

    private Task ExecuteCreateAsync()
    {
        return sut.ExecuteAsync(CreateCommentsCommand(new CreateComment { Text = "text1" }), default);
    }

    private Task ExecuteUpdateAsync()
    {
        return sut.ExecuteAsync(CreateCommentsCommand(new UpdateComment { Text = "text2" }), default);
    }

    private T CreateCommentsEvent<T>(T @event) where T : CommentsEvent
    {
        @event.Actor = actor;
        @event.CommentsId = commentsId;
        @event.CommentId = commentId;

        return @event;
    }

    private T CreateCommentsCommand<T>(T command) where T : CommentCommand
    {
        command.Actor = actor;
        command.CommentsId = commentsId;
        command.CommentId = commentId;

        return command;
    }

    private Instant GetTime()
    {
        return LastEvents.ElementAt(0).Headers.Timestamp();
    }
}
