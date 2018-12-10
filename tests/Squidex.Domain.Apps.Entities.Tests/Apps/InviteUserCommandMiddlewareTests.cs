// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Commands;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public class InviteUserCommandMiddlewareTests
    {
        private readonly IUserResolver userResolver = A.Fake<IUserResolver>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly InviteUserCommandMiddleware sut;

        public InviteUserCommandMiddlewareTests()
        {
            sut = new InviteUserCommandMiddleware(userResolver);
        }

        [Fact]
        public async Task Should_invite_user_and_change_result()
        {
            var command = new AssignContributor { ContributorId = "me@email.com", IsInviting = true };
            var context = new CommandContext(command, commandBus);

            A.CallTo(() => userResolver.CreateUserIfNotExists("me@email.com"))
                .Returns(true);

            var result = EntityCreatedResult.Create("13", 13L);

            context.Complete(result);

            await sut.HandleAsync(context);

            Assert.Same(context.Result<InvitedResult>().Id, result);

            A.CallTo(() => userResolver.CreateUserIfNotExists("me@email.com"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_invite_user_and_not_change_result_if_not_added()
        {
            var command = new AssignContributor { ContributorId = "me@email.com", IsInviting = true };
            var context = new CommandContext(command, commandBus);

            A.CallTo(() => userResolver.CreateUserIfNotExists("me@email.com"))
                .Returns(false);

            var result = EntityCreatedResult.Create("13", 13L);

            context.Complete(result);

            await sut.HandleAsync(context);

            Assert.Same(context.Result<EntityCreatedResult<string>>(), result);

            A.CallTo(() => userResolver.CreateUserIfNotExists("me@email.com"))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_calls_user_resolver_if_not_email()
        {
            var command = new AssignContributor { ContributorId = "123", IsInviting = true };
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            A.CallTo(() => userResolver.CreateUserIfNotExists(A<string>.Ignored))
                .MustNotHaveHappened();
        }
    }
}
