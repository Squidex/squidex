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
using Squidex.Domain.Apps.Entities.Apps.State;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Pipeline;
using Squidex.Pipeline.CommandMiddlewares;
using Xunit;
using static Squidex.Pipeline.AppApiFilter;

namespace Squidex.Tests.Pipeline.CommandMiddlewares
{
    public class EnrichWithAppIdCommandMiddlewareTests
    {
        private readonly IHttpContextAccessor httpContextAccessor = A.Fake<IHttpContextAccessor>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly CreateContent command = new CreateContent { AppId = null };

        [Fact]
        public async Task HandleAsync_should_throw_exception_if_app_id_not_found()
        {
            var context = new CommandContext(command, commandBus);
            var sut = SetupSystem(null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
            {
                return sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task HandleAsync_should_find_app_id_from_features()
        {
            var context = new CommandContext(command, commandBus);
            var app = new AppState
            {
                Name = "app",
                Id = Guid.NewGuid()
            };
            var sut = SetupSystem(app);

            await sut.HandleAsync(context);

            Assert.Equal(new NamedId<Guid>(app.Id, app.Name), command.AppId);
        }

        private EnrichWithAppIdCommandMiddleware SetupSystem(IAppEntity app)
        {
            var appFeature = app == null ? null : new AppFeature(app);
            A.CallTo(() => httpContextAccessor.HttpContext.Features.Get<IAppFeature>()).Returns(appFeature);

            return new EnrichWithAppIdCommandMiddleware(httpContextAccessor);
        }
    }
}
