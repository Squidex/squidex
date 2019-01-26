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
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly HttpContext httpContext = new DefaultHttpContext();
        private readonly EnrichWithAppIdCommandMiddleware sut;

        public EnrichWithAppIdCommandMiddlewareTests()
        {
            A.CallTo(() => httpContextAccessor.HttpContext)
                .Returns(httpContext);

            var appEntity = A.Fake<IAppEntity>();

            A.CallTo(() => appEntity.Id).Returns(appId.Id);
            A.CallTo(() => appEntity.Name).Returns(appId.Name);

            httpContext.Features.Set<IAppFeature>(new AppResolver.AppFeature(appEntity));

            sut = new EnrichWithAppIdCommandMiddleware(httpContextAccessor);
        }

        [Fact]
        public async Task Should_throw_exception_if_app_not_found()
        {
            httpContext.Features.Set<IAppFeature>(new AppResolver.AppFeature(null));

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
            var command = new CreateContent();
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.Equal(appId, command.AppId);
        }

        [Fact]
        public async Task Should_assign_app_id_to_app_self_command()
        {
            var command = new AddPattern();
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.Equal(appId.Id, command.AppId);
        }

        [Fact]
        public async Task Should_not_override_app_id()
        {
            var command = new AddPattern { AppId = Guid.NewGuid() };
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
