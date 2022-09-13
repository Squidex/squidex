// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Invitation
{
    public class InviteUserCommandMiddlewareTests
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly CancellationToken ct;
        private readonly IUserResolver userResolver = A.Fake<IUserResolver>();
        private readonly IAppEntity app = Mocks.App(NamedId.Of(DomainId.NewGuid(), "my-app"));
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly InviteUserCommandMiddleware sut;

        public InviteUserCommandMiddlewareTests()
        {
            ct = cts.Token;

            sut = new InviteUserCommandMiddleware(userResolver);
        }

        [Fact]
        public async Task Should_invite_user_and_change_actual_and_update_command()
        {
            var command = new AssignContributor { ContributorId = "me@email.com", Invite = true };

            var context =
                new CommandContext(command, commandBus)
                    .Complete(app);

            var user = UserMocks.User("123", command.ContributorId);

            A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(user.Email, true, ct))
                .Returns((user, true));

            await sut.HandleAsync(context, ct);

            Assert.Same(context.Result<InvitedResult<IAppEntity>>().Entity, app);
            Assert.Equal(user.Id, command.ContributorId);

            A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(user.Email, true, ct))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_invite_user_and_not_change_actual_if_not_added()
        {
            var command = new AssignContributor { ContributorId = "me@email.com", Invite = true };

            var context =
                new CommandContext(command, commandBus)
                    .Complete(app);

            var user = UserMocks.User("123", command.ContributorId);

            A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(user.Email, true, ct))
                .Returns((user, false));

            await sut.HandleAsync(context, ct);

            Assert.Same(context.Result<IAppEntity>(), app);
            Assert.Equal(user.Id, command.ContributorId);

            A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(user.Email, true, ct))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_call_user_resolver_if_not_email()
        {
            var command = new AssignContributor { ContributorId = "123", Invite = true };

            var context =
                new CommandContext(command, commandBus)
                    .Complete(app);

            await sut.HandleAsync(context, ct);

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

            await sut.HandleAsync(context, ct);

            A.CallTo(() => userResolver.CreateUserIfNotExistsAsync(A<string>._, A<bool>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }
    }
}
