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
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Infrastructure.Commands;
using Squidex.Pipeline.CommandMiddlewares;
using Xunit;

namespace Squidex.Tests.Pipeline.CommandMiddlewares
{
    public class ETagCommandMiddlewareTests
    {
        private readonly IHttpContextAccessor httpContextAccessor = A.Fake<IHttpContextAccessor>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly IHeaderDictionary headers = new HeaderDictionary { { "If-Match", "1" } };
        private readonly UpdateAsset command = new UpdateAsset();
        private readonly EntitySavedResult entitySavedResult = new EntitySavedResult(1);
        private readonly ETagCommandMiddleware sut;

        public ETagCommandMiddlewareTests()
        {
            A.CallTo(() => httpContextAccessor.HttpContext.Request.Headers).Returns(headers);
            sut = new ETagCommandMiddleware(httpContextAccessor);
        }

        [Fact]
        public async Task Should_add_etag_header_and_expected_version()
        {
            var context = new CommandContext(command, commandBus);
            context.Complete(entitySavedResult);

            await sut.HandleAsync(context);

            Assert.Equal(1, context.Command.ExpectedVersion);
            Assert.Equal(new StringValues("1"), httpContextAccessor.HttpContext.Response.Headers["ETag"]);
        }
    }
}
