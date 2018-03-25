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
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure.Commands;
using Xunit;

namespace Squidex.Pipeline.CommandMiddlewares
{
    public class ETagCommandMiddlewareTests
    {
        private readonly IHttpContextAccessor httpContextAccessor = A.Fake<IHttpContextAccessor>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly IHeaderDictionary requestHeaders = new HeaderDictionary();
        private readonly ETagCommandMiddleware sut;

        public ETagCommandMiddlewareTests()
        {
            A.CallTo(() => httpContextAccessor.HttpContext.Request.Headers)
                .Returns(requestHeaders);

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
            requestHeaders["If-Match"] = "13";

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

            Assert.Equal(new StringValues("17"), httpContextAccessor.HttpContext.Response.Headers["ETag"]);
        }
    }
}
