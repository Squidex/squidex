﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Tasks;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Comments
{
    public class CommentsCommandMiddlewareTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly IUserResolver userResolver = A.Fake<IUserResolver>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly RefToken actor = new RefToken(RefTokenType.Subject, "me");
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly Guid commentsId = Guid.NewGuid();
        private readonly Guid commentId = Guid.NewGuid();
        private readonly CommentsCommandMiddleware sut;

        public CommentsCommandMiddlewareTests()
        {
            A.CallTo(() => userResolver.FindByIdOrEmailAsync(A<string>.Ignored))
                .Returns(Task.FromResult<IUser?>(null));

            sut = new CommentsCommandMiddleware(grainFactory, userResolver);
        }

        [Fact]
        public async Task Should_invoke_grain_for_comments_command()
        {
            var command = CreateCommentsCommand(new CreateComment());
            var context = CreateContextForCommand(command);

            var grain = A.Fake<ICommentsGrain>();

            var result = "Completed";

            A.CallTo(() => grainFactory.GetGrain<ICommentsGrain>(commentsId.ToString(), null))
                .Returns(grain);

            A.CallTo(() => grain.ExecuteAsync(A<J<CommentsCommand>>.That.Matches(x => x.Value == command)))
                .Returns(new J<object>(result));

            var isNextCalled = false;

            await sut.HandleAsync(context, () =>
            {
                isNextCalled = true;

                return TaskHelper.Done;
            });

            Assert.True(isNextCalled);

            A.CallTo(() => grain.ExecuteAsync(A<J<CommentsCommand>>.That.Matches(x => x.Value == command)))
                .Returns(new J<object>(12));
        }

        [Fact]
        public async Task Should_enrich_with_mentioned_user_ids_if_found()
        {
            SetupUser("id1", "mail1@squidex.io");
            SetupUser("id2", "mail2@squidex.io");

            var command = CreateCommentsCommand(new CreateComment
            {
                Text = "Hi @mail1@squidex.io, @mail2@squidex.io and @notfound@squidex.io"
            });

            var context = CreateContextForCommand(command);

            await sut.HandleAsync(context);

            Assert.Equal(command.Mentions, new[] { "id1", "id2" });
        }

        [Fact]
        public async Task Should_invoke_commands_for_mentioned_users()
        {
            SetupUser("id1", "mail1@squidex.io");
            SetupUser("id2", "mail2@squidex.io");

            var command = CreateCommentsCommand(new CreateComment
            {
                Text = "Hi @mail1@squidex.io and @mail2@squidex.io"
            });

            var context = CreateContextForCommand(command);

            await sut.HandleAsync(context);

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>.That.Matches(x => IsForUser(x, "id1"))))
                .MustHaveHappened();

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>.That.Matches(x => IsForUser(x, "id2"))))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_enrich_with_mentioned_user_ids_if_invalid_mentioned_tags_used()
        {
            var command = CreateCommentsCommand(new CreateComment
            {
                Text = "Hi invalid@squidex.io"
            });

            var context = CreateContextForCommand(command);

            await sut.HandleAsync(context);

            A.CallTo(() => userResolver.FindByIdOrEmailAsync(A<string>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_enrich_with_mentioned_user_ids_for_notification()
        {
            var command = new CreateComment
            {
                Text = "Hi @invalid@squidex.io", IsMention = true
            };

            var context = CreateContextForCommand(command);

            await sut.HandleAsync(context);

            A.CallTo(() => userResolver.FindByIdOrEmailAsync(A<string>.Ignored))
                .MustNotHaveHappened();
        }

        protected CommandContext CreateContextForCommand<TCommand>(TCommand command) where TCommand : CommentsCommand
        {
            return new CommandContext(command, commandBus);
        }

        private static bool IsForUser(ICommand command, string id)
        {
            return command is CreateComment createComment &&
                createComment.CommentsId == id &&
                createComment.Mentions == null &&
                createComment.AppId == null &&
                createComment.ExpectedVersion == EtagVersion.Any &&
                createComment.IsMention;
        }

        private void SetupUser(string id, string email)
        {
            var user = A.Fake<IUser>();

            A.CallTo(() => user.Id).Returns(id);
            A.CallTo(() => user.Email).Returns(email);

            A.CallTo(() => userResolver.FindByIdOrEmailAsync(email))
                .Returns(user);
        }

        protected T CreateCommentsCommand<T>(T command) where T : CommentsCommand
        {
            command.Actor = actor;
            command.AppId = appId;
            command.CommentsId = commentsId.ToString();
            command.CommentId = commentId;

            return command;
        }
    }
}
