// ==========================================================================
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
using Squidex.Domain.Apps.Core.Comments;
using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Domain.Apps.Entities.Comments.State;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Comments;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Comments
{
    public class CommentsGrainTests : HandlerTestBase<CommentsState>
    {
        private readonly Guid commentsId = Guid.NewGuid();
        private readonly Guid commentId = Guid.NewGuid();
        private readonly CommentsGrain sut;

        protected override Guid Id
        {
            get { return commentsId; }
        }

        public CommentsGrainTests()
        {
            sut = new CommentsGrain(Store, A.Dummy<ISemanticLog>());
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
                    new Comment(command.CommentId, LastEvents.ElementAt(0).Headers.Timestamp(), command.Actor, "text1")
                },
                Version = 0
            });

            LastEvents
                .ShouldHaveSameEvents(
                    CreateCommentsEvent(new CommentCreated { Text = command.Text, Url = new Uri("http://uri") })
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
                    new Comment(commentId, LastEvents.ElementAt(0).Headers.Timestamp(), updateCommand.Actor, "text2")
                },
                Version = 1
            });

            sut.GetCommentsAsync(0).Result.Should().BeEquivalentTo(new CommentsResult
            {
                UpdatedComments = new List<Comment>
                {
                    new Comment(commentId, LastEvents.ElementAt(0).Headers.Timestamp(), updateCommand.Actor, "text2")
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
            @event.CommentsId = commentsId;
            @event.CommentId = commentId;

            return CreateEvent(@event);
        }

        protected T CreateCommentsCommand<T>(T command) where T : CommentsCommand
        {
            command.CommentsId = commentsId;
            command.CommentId = commentId;

            return CreateCommand(command);
        }
    }
}