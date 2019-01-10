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
            var command = new CreateComment { Text = "text1" };

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
                    CreateCommentsEvent(new CommentCreated { CommentId = command.CommentId, Text = command.Text })
                );
        }

        [Fact]
        public async Task Update_should_create_events_and_update_state()
        {
            var createCommand = new CreateComment { Text = "text1" };
            var updateCommand = new UpdateComment { Text = "text2", CommentId = createCommand.CommentId };

            await sut.ExecuteAsync(CreateCommentsCommand(createCommand));

            var result = await sut.ExecuteAsync(CreateCommentsCommand(updateCommand));

            result.ShouldBeEquivalent(new EntitySavedResult(1));

            sut.GetCommentsAsync(-1).Result.Should().BeEquivalentTo(new CommentsResult
            {
                CreatedComments = new List<Comment>
                {
                    new Comment(createCommand.CommentId, LastEvents.ElementAt(0).Headers.Timestamp(), createCommand.Actor, "text2")
                },
                Version = 1
            });

            sut.GetCommentsAsync(0).Result.Should().BeEquivalentTo(new CommentsResult
            {
                UpdatedComments = new List<Comment>
                {
                    new Comment(createCommand.CommentId, LastEvents.ElementAt(0).Headers.Timestamp(), createCommand.Actor, "text2")
                },
                Version = 1
            });

            LastEvents
                .ShouldHaveSameEvents(
                    CreateCommentsEvent(new CommentUpdated { CommentId = createCommand.CommentId, Text = updateCommand.Text })
                );
        }

        [Fact]
        public async Task Delete_should_create_events_and_update_state()
        {
            var createCommand = new CreateComment { Text = "text1" };
            var updateCommand = new UpdateComment { Text = "text2", CommentId = createCommand.CommentId };
            var deleteCommand = new DeleteComment { CommentId = createCommand.CommentId };

            await sut.ExecuteAsync(CreateCommentsCommand(createCommand));
            await sut.ExecuteAsync(CreateCommentsCommand(updateCommand));

            var result = await sut.ExecuteAsync(CreateCommentsCommand(deleteCommand));

            result.ShouldBeEquivalent(new EntitySavedResult(2));

            sut.GetCommentsAsync(-1).Result.Should().BeEquivalentTo(new CommentsResult { Version = 2 });
            sut.GetCommentsAsync(0).Result.Should().BeEquivalentTo(new CommentsResult
            {
                DeletedComments = new List<Guid>
                {
                    deleteCommand.CommentId
                },
                Version = 2
            });
            sut.GetCommentsAsync(1).Result.Should().BeEquivalentTo(new CommentsResult
            {
                DeletedComments = new List<Guid>
                {
                    deleteCommand.CommentId
                },
                Version = 2
            });

            LastEvents
                .ShouldHaveSameEvents(
                    CreateCommentsEvent(new CommentDeleted { CommentId = createCommand.CommentId })
                );
        }

        protected T CreateCommentsEvent<T>(T @event) where T : CommentsEvent
        {
            @event.CommentsId = commentsId;

            return CreateEvent(@event);
        }

        protected T CreateCommentsCommand<T>(T command) where T : CommentsCommand
        {
            command.CommentsId = commentsId;

            return CreateCommand(command);
        }
    }
}