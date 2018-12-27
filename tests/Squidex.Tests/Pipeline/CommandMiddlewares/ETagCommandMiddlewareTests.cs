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
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure.Commands;
using Xunit;

namespace Squidex.Pipeline.CommandMiddlewares
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
                .Returns(null);

            var command = new CreateContent();
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.Null(command.Actor);
        }

        [Fact]
        public async Task Should_add_expected_version_to_command()
        {
            httpContext.Request.Headers[HeaderNames.IfMatch] = "13";

            var command = new CreateContent();
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.Equal(13, context.Command.ExpectedVersion);
        }

        [Fact]
        public async Task Should_add_weak_etag_as_expected_version_to_command()
        {
            httpContext.Request.Headers[HeaderNames.IfMatch] = "W/13";

            var command = new CreateContent();
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.Equal(13, context.Command.ExpectedVersion);
        }

        [Fact]
        public async Task Should_add_etag_header_to_response()
        {
            var command = new CreateContent();
            var context = new CommandContext(command, commandBus);

            context.Complete(new EntitySavedResult(17));

            await sut.HandleAsync(context);

            Assert.Equal(new StringValues("17"), httpContextAccessor.HttpContext.Response.Headers[HeaderNames.ETag]);
        }
    }
}
