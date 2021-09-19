// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Comments.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Comments.DomainObject
{
    public class CommentsCommandMiddlewareTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly IUserResolver userResolver = A.Fake<IUserResolver>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly RefToken actor = RefToken.User("me");
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly DomainId commentsId = DomainId.NewGuid();
        private readonly DomainId commentId = DomainId.NewGuid();
        private readonly CommentsCommandMiddleware sut;

        public CommentsCommandMiddlewareTests()
        {
            A.CallTo(() => userResolver.FindByIdOrEmailAsync(A<string>._, default))
                .Returns(Task.FromResult<IUser?>(null));

            sut = new CommentsCommandMiddleware(grainFactory, userResolver);
        }

        [Fact]
        public async Task Should_invoke_grain_for_comments_command()
        {
            var command = CreateCommentsCommand(new CreateComment());
            var context = CrateCommandContext(command);

            var grain = A.Fake<ICommentsGrain>();

            A.CallTo(() => grainFactory.GetGrain<ICommentsGrain>(commentsId.ToString(), null))
                .Returns(grain);

            A.CallTo(() => grain.ExecuteAsync(A<J<CommentsCommand>>.That.Matches(x => x.Value == command)))
                .Returns(CommandResult.Empty(commentsId, 0, 0).AsJ());

            var isNextCalled = false;

            await sut.HandleAsync(context, c =>
            {
                isNextCalled = true;

                return Task.CompletedTask;
            });

            Assert.True(isNextCalled);
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

            var context = CrateCommandContext(command);

            await sut.HandleAsync(context);

            Assert.Equal(command.Mentions, new[] { "id1", "id2" });
        }

        [Fact]
        public async Task Should_not_invoke_commands_for_mentioned_users()
        {
            SetupUser("id1", "mail1@squidex.io");
            SetupUser("id2", "mail2@squidex.io");

            var command = CreateCommentsCommand(new CreateComment
            {
                Text = "Hi @mail1@squidex.io and @mail2@squidex.io"
            });

            var context = CrateCommandContext(command);

            await sut.HandleAsync(context);

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_enrich_with_mentioned_user_ids_if_invalid_mentioned_tags_used()
        {
            var command = CreateCommentsCommand(new CreateComment
            {
                Text = "Hi invalid@squidex.io"
            });

            var context = CrateCommandContext(command);

            await sut.HandleAsync(context);

            A.CallTo(() => userResolver.FindByIdOrEmailAsync(A<string>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_enrich_with_mentioned_user_ids_for_notification()
        {
            var command = new CreateComment
            {
                Text = "Hi @invalid@squidex.io", IsMention = true
            };

            var context = CrateCommandContext(command);

            await sut.HandleAsync(context);

            A.CallTo(() => userResolver.FindByIdOrEmailAsync(A<string>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        private CommandContext CrateCommandContext(ICommand command)
        {
            return new CommandContext(command, commandBus);
        }

        private void SetupUser(string id, string email)
        {
            var user = UserMocks.User(id, email);

            A.CallTo(() => userResolver.FindByIdOrEmailAsync(email, default))
                .Returns(user);
        }

        private T CreateCommentsCommand<T>(T command) where T : CommentsCommand
        {
            command.Actor = actor;
            command.AppId = appId;
            command.CommentsId = commentsId;
            command.CommentId = commentId;

            return command;
        }
    }
}
