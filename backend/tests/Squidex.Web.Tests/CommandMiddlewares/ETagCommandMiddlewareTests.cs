// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure.Commands;
using Xunit;

namespace Squidex.Web.CommandMiddlewares
{
    public class ETagCommandMiddlewareTests
    {
        private readonly IHttpContextAccessor httpContextAccessor = A.Fake<IHttpContextAccessor>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly HttpContext httpContext = new DefaultHttpContext();
        private readonly ETagCommandMiddleware sut;

        public ETagCommandMiddlewareTests()
        {
            A.CallTo(() => httpContextAccessor.HttpContext)
                .Returns(httpContext);

            sut = new ETagCommandMiddleware(httpContextAccessor);
        }

        [Fact]
        public async Task Should_do_nothing_when_context_is_null()
        {
            A.CallTo(() => httpContextAccessor.HttpContext)
                .Returns(null!);

            var command = new CreateContent();
            var context = Ctx(command);

            await sut.HandleAsync(context);

            Assert.Null(command.Actor);
        }

        [Fact]
        public async Task Should_do_nothing_if_command_has_etag_defined()
        {
            httpContext.Request.Headers[HeaderNames.IfMatch] = "13";

            var command = new CreateContent { ExpectedVersion = 1 };
            var context = Ctx(command);

            await sut.HandleAsync(context);

            Assert.Equal(1, context.Command.ExpectedVersion);
        }

        [Fact]
        public async Task Should_add_expected_version_to_command()
        {
            httpContext.Request.Headers[HeaderNames.IfMatch] = "13";

            var command = new CreateContent();
            var context = Ctx(command);

            await sut.HandleAsync(context);

            Assert.Equal(13, context.Command.ExpectedVersion);
        }

        [Fact]
        public async Task Should_add_weak_etag_as_expected_version_to_command()
        {
            httpContext.Request.Headers[HeaderNames.IfMatch] = "W/13";

            var command = new CreateContent();
            var context = Ctx(command);

            await sut.HandleAsync(context);

            Assert.Equal(13, context.Command.ExpectedVersion);
        }

        [Fact]
        public async Task Should_add_version_from_result_as_etag_to_response()
        {
            var command = new CreateContent();
            var context = Ctx(command);

            context.Complete(new EntitySavedResult(17));

            await sut.HandleAsync(context);

            Assert.Equal(new StringValues("17"), httpContextAccessor.HttpContext!.Response.Headers[HeaderNames.ETag]);
        }

        [Fact]
        public async Task Should_add_version_from_entity_as_etag_to_response()
        {
            var command = new CreateContent();
            var context = Ctx(command);

            context.Complete(new ContentEntity { Version = 17 });

            await sut.HandleAsync(context);

            Assert.Equal(new StringValues("17"), httpContextAccessor.HttpContext!.Response.Headers[HeaderNames.ETag]);
        }

        private CommandContext Ctx(ICommand command)
        {
            return new CommandContext(command, commandBus);
        }
    }
}
