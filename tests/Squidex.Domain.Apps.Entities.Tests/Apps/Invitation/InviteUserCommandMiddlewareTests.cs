﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
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

            A.CallTo(() => userResolver.CreateUserIfNotExists("me@email.com", true))
                .Returns(true);

            var result = Mocks.App(NamedId.Of(Guid.NewGuid(), "my-app"));

            context.Complete(result);

            await sut.HandleAsync(context);

            Assert.Same(context.Result<InvitedResult>().App, result);

            A.CallTo(() => userResolver.CreateUserIfNotExists("me@email.com", true))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_invite_user_and_not_change_result_if_not_added()
        {
            var command = new AssignContributor { ContributorId = "me@email.com", IsInviting = true };
            var context = new CommandContext(command, commandBus);

            A.CallTo(() => userResolver.CreateUserIfNotExists("me@email.com", true))
                .Returns(false);

            var result = Mocks.App(NamedId.Of(Guid.NewGuid(), "my-app"));

            context.Complete(result);

            await sut.HandleAsync(context);

            Assert.Same(context.Result<IAppEntity>(), result);

            A.CallTo(() => userResolver.CreateUserIfNotExists("me@email.com", true))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_calls_user_resolver_if_not_email()
        {
            var command = new AssignContributor { ContributorId = "123", IsInviting = true };
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            A.CallTo(() => userResolver.CreateUserIfNotExists(A<string>.Ignored, A<bool>.Ignored))
                .MustNotHaveHappened();
        }
    }
}
