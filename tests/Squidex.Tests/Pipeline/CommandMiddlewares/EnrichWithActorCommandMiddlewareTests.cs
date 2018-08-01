// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Xunit;

namespace Squidex.Pipeline.CommandMiddlewares
{
    public class EnrichWithActorCommandMiddlewareTests
    {
        private readonly IHttpContextAccessor httpContextAccessor = A.Fake<IHttpContextAccessor>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly HttpContext httpContext = new DefaultHttpContext();
        private readonly EnrichWithActorCommandMiddleware sut;

        public EnrichWithActorCommandMiddlewareTests()
        {
            A.CallTo(() => httpContextAccessor.HttpContext)
                .Returns(httpContext);

            sut = new EnrichWithActorCommandMiddleware(httpContextAccessor);
        }

        [Fact]
        public async Task Should_throw_security_exception_when_no_subject_or_client_is_found()
        {
            var command = new CreateContent();
            var context = new CommandContext(command, commandBus);

            await Assert.ThrowsAsync<SecurityException>(() => sut.HandleAsync(context));
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
        public async Task Should_assign_actor_from_subject()
        {
            httpContext.User = CreatePrincipal(OpenIdClaims.Subject, "me");

            var command = new CreateContent();
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.Equal(new RefToken(RefTokenType.Subject, "me"), command.Actor);
        }

        [Fact]
        public async Task Should_assign_actor_from_client()
        {
            httpContext.User = CreatePrincipal(OpenIdClaims.ClientId, "my-client");

            var command = new CreateContent();
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.Equal(new RefToken(RefTokenType.Client, "my-client"), command.Actor);
        }

        [Fact]
        public async Task Should_not_override_actor()
        {
            httpContext.User = CreatePrincipal(OpenIdClaims.ClientId, "my-client");

            var command = new CreateContent { Actor = new RefToken("subject", "me") };
            var context = new CommandContext(command, commandBus);

            await sut.HandleAsync(context);

            Assert.Equal(new RefToken("subject", "me"), command.Actor);
        }

        private static ClaimsPrincipal CreatePrincipal(string claimType, string claimValue)
        {
            var claimsPrincipal = new ClaimsPrincipal();
            var claimsIdentity = new ClaimsIdentity();

            claimsIdentity.AddClaim(new Claim(claimType, claimValue));
            claimsPrincipal.AddIdentity(claimsIdentity);

            return claimsPrincipal;
        }
    }
}
