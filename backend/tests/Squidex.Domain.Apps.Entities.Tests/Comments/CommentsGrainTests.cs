﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using NodaTime;
using Squidex.Domain.Apps.Core.Comments;
using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Comments;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Comments
{
    public class CommentsGrainTests
    {
        private readonly IEventStore eventStore = A.Fake<IEventStore>();
        private readonly IEventDataFormatter eventDataFormatter = A.Fake<IEventDataFormatter>();
        private readonly Guid commentsId = Guid.NewGuid();
        private readonly Guid commentId = Guid.NewGuid();
        private readonly RefToken actor = new RefToken(RefTokenType.Subject, "me");
        private readonly CommentsGrain sut;

        private string Id
        {
            get { return commentsId.ToString(); }
        }

        public IEnumerable<Envelope<IEvent>> LastEvents { get; private set; } = Enumerable.Empty<Envelope<IEvent>>();

        public CommentsGrainTests()
        {
            A.CallTo(() => eventStore.AppendAsync(A<Guid>.Ignored, A<string>.Ignored, A<long>.Ignored, A<ICollection<EventData>>.Ignored))
                .Invokes(x => LastEvents = sut.GetUncommittedEvents().Select(x => x.To<IEvent>()).ToList());

            sut = new CommentsGrain(eventStore, eventDataFormatter);
            sut.ActivateAsync(Id).Wait();
        }

        [Fact]
        public async Task Create_should_create_events()
        {
            var command = new CreateComment { Text = "text1", Url = new Uri("http://uri") };

            var result = await sut.ExecuteAsync(CreateCommentsCommand(command));

            result.ShouldBeEquivalent(EntityCreatedResult.Create(command.CommentId, 0));

            sut.GetCommentsAsync(0).Result.Should().BeEquivalentTo(new CommentsResult { Version = 0 });
            sut.GetCommentsAsync(-1).Result.Should().BeEquivalentTo(new CommentsResult
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
        public async Task Update_should_create_events_and_update_state()
        {
            await ExecuteCreateAsync();

            var updateCommand = new UpdateComment { Text = "text2" };

            var result = await sut.ExecuteAsync(CreateCommentsCommand(updateCommand));

            result.ShouldBeEquivalent(new EntitySavedResult(1));

            sut.GetCommentsAsync(-1).Result.Should().BeEquivalentTo(new CommentsResult
            {
                CreatedComments = new List<Comment>
                {
                    new Comment(commentId, GetTime(), updateCommand.Actor, "text2")
                },
                Version = 1
            });

            sut.GetCommentsAsync(0).Result.Should().BeEquivalentTo(new CommentsResult
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
        public async Task Delete_should_create_events_and_update_state()
        {
            await ExecuteCreateAsync();
            await ExecuteUpdateAsync();

            var deleteCommand = new DeleteComment();

            var result = await sut.ExecuteAsync(CreateCommentsCommand(deleteCommand));

            result.ShouldBeEquivalent(new EntitySavedResult(2));

            sut.GetCommentsAsync(-1).Result.Should().BeEquivalentTo(new CommentsResult { Version = 2 });
            sut.GetCommentsAsync(0).Result.Should().BeEquivalentTo(new CommentsResult
            {
                DeletedComments = new List<Guid>
                {
                    commentId
                },
                Version = 2
            });
            sut.GetCommentsAsync(1).Result.Should().BeEquivalentTo(new CommentsResult
            {
                DeletedComments = new List<Guid>
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
            return sut.ExecuteAsync(CreateCommentsCommand(new CreateComment { Text = "text1" }));
        }

        private Task ExecuteUpdateAsync()
        {
            return sut.ExecuteAsync(CreateCommentsCommand(new UpdateComment { Text = "text2" }));
        }

        protected T CreateCommentsEvent<T>(T @event) where T : CommentsEvent
        {
            @event.Actor = actor;
            @event.CommentsId = commentsId.ToString();
            @event.CommentId = commentId;

            return @event;
        }

        protected T CreateCommentsCommand<T>(T command) where T : CommentsCommand
        {
            command.Actor = actor;
            command.CommentsId = commentsId.ToString();
            command.CommentId = commentId;

            return command;
        }

        private Instant GetTime()
        {
            return LastEvents.ElementAt(0).Headers.Timestamp();
        }
    }
}