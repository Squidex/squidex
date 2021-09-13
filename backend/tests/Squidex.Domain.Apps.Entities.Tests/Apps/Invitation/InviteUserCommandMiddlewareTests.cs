// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Invitation
{
    public class InviteUserCommandMiddlewareTests
    {
        private readonly IUserResolver userResolver = A.Fake<IUserResolver>();
        private readonly IAppEntity app = Mocks.App(NamedId.Of(DomainId.NewGuid(), "my-app"));
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly InviteUserCommandMiddleware sut;

        public InviteUserCommandMiddlewareTests()
        {
            sut = new InviteUserCommandMiddleware(userResolver);
        }

        [Fact]
        public async Task Should_invite_user_and_change_result_and_update_command()
        {
            var command = new AssignContributor { ContributorId = "me@email.com", Invite = true };

            var context =
                new CommandContext(command, commandBus)
                    .Complete(app);

            var user = UserMocks.User("123", command.ContributorId);

            A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(user.Email, true, default))
                .Returns((user, true));

            await sut.HandleAsync(context);

            Assert.Same(context.Result<InvitedResult>().App, app);
            Assert.Equal(user.Id, command.ContributorId);

            A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(user.Email, true, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_invite_user_and_not_change_result_if_not_added()
        {
            var command = new AssignContributor { ContributorId = "me@email.com", Invite = true };

            var context =
                new CommandContext(command, commandBus)
                    .Complete(app);

            var user = UserMocks.User("123", command.ContributorId);

            A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(user.Email, true, default))
                .Returns((user, false));

            await sut.HandleAsync(context);

            Assert.Same(context.Result<IAppEntity>(), app);
            Assert.Equal(user.Id, command.ContributorId);

            A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(user.Email, true, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_call_user_resolver_if_not_email()
        {
            var command = new AssignContributor { ContributorId = "123", Invite = true };

            var context =
                new CommandContext(command, commandBus)
                    .Complete(app);

            await sut.HandleAsync(context);

            A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(A<string>._, A<bool>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_call_user_resolver_if_not_inviting()
        {
            var command = new AssignContributor { ContributorId = "123", Invite = false };

            var context =
                new CommandContext(command, commandBus)
                    .Complete(app);

            await sut.HandleAsync(context);

            A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(A<string>._, A<bool>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }
    }
}
