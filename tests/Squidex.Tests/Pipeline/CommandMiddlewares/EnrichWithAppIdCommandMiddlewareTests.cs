// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Xunit;

namespace Squidex.Pipeline.CommandMiddlewares
{
    public class EnrichWithAppIdCommandMiddlewareTests
    {
        private readonly IHttpContextAccessor httpContextAccessor = A.Fake<IHttpContextAccessor>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly HttpContext httpContext = new DefaultHttpContext();
        private readonly EnrichWithAppIdCommandMiddleware sut;

        public EnrichWithAppIdCommandMiddlewareTests()
        {
            A.CallTo(() => httpContextAccessor.HttpContext)
                .Returns(httpContext);

            sut = new EnrichWithAppIdCommandMiddleware(httpContextAccessor);
        }

        [Fact]
        public async Task Should_throw_exception_if_app_not_found()
        {
            var command = new CreateContent();
            var context = new CommandContext(command, commandBus);

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.HandleAsync(context));
        }

        [Fact]
        public async Task Should_do_nothing_when_context_is_null()
        {
            A.CallTo(() => httpContextAccessor.HttpContext)
                .Returns(null);

            var command = new CreateContent();
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.Null(command.Actor);
        }

        [Fact]
        public async Task Should_assign_app_id_and_name_to_app_command()
        {
            SetupApp(out var appId, out var appName);

            var command = new CreateContent();
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.Equal(NamedId.Of(appId, appName), command.AppId);
        }

        [Fact]
        public async Task Should_assign_app_id_to_app_self_command()
        {
            SetupApp(out var appId, out _);

            var command = new AddPattern();
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.Equal(appId, command.AppId);
        }

        [Fact]
        public async Task Should_not_override_app_id()
        {
            SetupApp(out var appId, out _);

            var command = new AddPattern { AppId = Guid.NewGuid() };
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.NotEqual(appId, command.AppId);
        }

        [Fact]
        public async Task Should_not_override_app_id_and_name()
        {
            SetupApp(out var appId, out var appName);

            var command = new CreateContent { AppId = NamedId.Of(Guid.NewGuid(), "other-app") };
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.NotEqual(NamedId.Of(appId, appName), command.AppId);
        }

        private void SetupApp(out Guid appId, out string appName)
        {
            appId = Guid.NewGuid();
            appName = "my-app";

            var appEntity = A.Fake<IAppEntity>();
            A.CallTo(() => appEntity.Id).Returns(appId);
            A.CallTo(() => appEntity.Name).Returns(appName);

            httpContext.Features.Set<IAppFeature>(new AppApiFilter.AppFeature(appEntity));
        }
    }
}
