// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Xunit;

namespace Squidex.Web.CommandMiddlewares
{
    public class EnrichWithAppIdCommandMiddlewareTests
    {
        private readonly IContextProvider contextProvider = A.Fake<IContextProvider>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly Context appContext = new Context();
        private readonly EnrichWithAppIdCommandMiddleware sut;

        public EnrichWithAppIdCommandMiddlewareTests()
        {
            A.CallTo(() => contextProvider.Context)
                .Returns(appContext);

            var app = A.Fake<IAppEntity>();

            A.CallTo(() => app.Id).Returns(appId.Id);
            A.CallTo(() => app.Name).Returns(appId.Name);

            appContext.App = app;

            sut = new EnrichWithAppIdCommandMiddleware(contextProvider);
        }

        [Fact]
        public async Task Should_throw_exception_if_app_not_found()
        {
            appContext.App = null;

            var command = new CreateContent();
            var context = new CommandContext(command, commandBus);

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.HandleAsync(context));
        }

        [Fact]
        public async Task Should_assign_app_id_and_name_to_app_command()
        {
            var command = new CreateContent();
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.Equal(appId, command.AppId);
        }

        [Fact]
        public async Task Should_assign_app_id_to_app_self_command()
        {
            var command = new ChangePlan();
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.Equal(appId.Id, command.AppId);
        }

        [Fact]
        public async Task Should_not_override_app_id()
        {
            var command = new ChangePlan { AppId = Guid.NewGuid() };
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.NotEqual(appId.Id, command.AppId);
        }

        [Fact]
        public async Task Should_not_override_app_id_and_name()
        {
            var command = new CreateContent { AppId = NamedId.Of(Guid.NewGuid(), "other-app") };
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.NotEqual(appId, command.AppId);
        }
    }
}
