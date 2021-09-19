// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Validation;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Plans
{
    public sealed class RestrictAppsCommandMiddlewareTests
    {
        private readonly IUserResolver userResolver = A.Fake<IUserResolver>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly RestrictAppsOptions options = new RestrictAppsOptions();
        private readonly RestrictAppsCommandMiddleware sut;

        public RestrictAppsCommandMiddlewareTests()
        {
            sut = new RestrictAppsCommandMiddleware(Options.Create(options), userResolver);
        }

        [Fact]
        public async Task Should_throw_exception_if_number_of_apps_reached()
        {
            var userId = Guid.NewGuid().ToString();

            var command = new CreateApp
            {
                Actor = RefToken.User(userId)
            };

            var commandContext = new CommandContext(command, commandBus);

            options.MaximumNumberOfApps = 3;

            var user = A.Fake<IUser>();

            A.CallTo(() => user.Id)
                .Returns(userId);

            A.CallTo(() => user.Claims)
                .Returns(Enumerable.Repeat(new Claim(SquidexClaimTypes.TotalApps, "5"), 1).ToList());

            A.CallTo(() => userResolver.FindByIdAsync(userId, default))
                .Returns(user);

            var isNextCalled = false;

            await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(commandContext, x =>
            {
                isNextCalled = true;

                return Task.CompletedTask;
            }));

            Assert.False(isNextCalled);
        }

        [Fact]
        public async Task Should_increment_total_apps_if_maximum_not_reached_and_completed()
        {
            var userId = Guid.NewGuid().ToString();

            var command = new CreateApp
            {
                Actor = RefToken.User(userId)
            };

            var commandContext = new CommandContext(command, commandBus);

            options.MaximumNumberOfApps = 10;

            var user = A.Fake<IUser>();

            A.CallTo(() => user.Id)
                .Returns(userId);

            A.CallTo(() => user.Claims)
                .Returns(Enumerable.Repeat(new Claim(SquidexClaimTypes.TotalApps, "5"), 1).ToList());

            A.CallTo(() => userResolver.FindByIdAsync(userId, default))
                .Returns(user);

            await sut.HandleAsync(commandContext, x =>
            {
                x.Complete(true);

                return Task.CompletedTask;
            });

            A.CallTo(() => userResolver.SetClaimAsync(userId, SquidexClaimTypes.TotalApps, "6", true, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_check_usage_if_app_is_created_by_client()
        {
            var command = new CreateApp
            {
                Actor = RefToken.Client(Guid.NewGuid().ToString())
            };

            var commandContext = new CommandContext(command, commandBus);

            options.MaximumNumberOfApps = 10;

            await sut.HandleAsync(commandContext, x =>
            {
                x.Complete(true);

                return Task.CompletedTask;
            });

            A.CallTo(() => userResolver.FindByIdAsync(A<string>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_check_usage_if_no_maximum_configured()
        {
            var command = new CreateApp
            {
                Actor = RefToken.User(Guid.NewGuid().ToString())
            };

            var commandContext = new CommandContext(command, commandBus);

            options.MaximumNumberOfApps = 0;

            await sut.HandleAsync(commandContext, x =>
            {
                x.Complete(true);

                return Task.CompletedTask;
            });

            A.CallTo(() => userResolver.FindByIdAsync(A<string>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_check_usage_for_other_commands()
        {
            var command = new UpdateApp
            {
                Actor = RefToken.User(Guid.NewGuid().ToString())
            };

            var commandContext = new CommandContext(command, commandBus);

            options.MaximumNumberOfApps = 10;

            await sut.HandleAsync(commandContext, x =>
            {
                x.Complete(true);

                return Task.CompletedTask;
            });

            A.CallTo(() => userResolver.FindByIdAsync(A<string>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }
    }
}
