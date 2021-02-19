﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Xunit;

namespace Squidex.Web.CommandMiddlewares
{
    public class EnrichWithActorCommandMiddlewareTests
    {
        private readonly IHttpContextAccessor httpContextAccessor = A.Fake<IHttpContextAccessor>();
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
            await Assert.ThrowsAsync<DomainForbiddenException>(() => HandleAsync(new CreateContent()));
        }

        [Fact]
        public async Task Should_do_nothing_when_context_is_null()
        {
            A.CallTo(() => httpContextAccessor.HttpContext)
                .Returns(null!);

            var context =
                await HandleAsync(
                    new CreateContent());

            Assert.Null(((SquidexCommand)context.Command).Actor);
        }

        [Fact]
        public async Task Should_assign_actor_from_subject()
        {
            httpContext.User = CreatePrincipal(OpenIdClaims.Subject, "my-user");

            var context = await HandleAsync(new CreateContent());

            Assert.Equal(RefToken.User("my-user"), ((SquidexCommand)context.Command).Actor);
        }

        [Fact]
        public async Task Should_assign_actor_from_client()
        {
            httpContext.User = CreatePrincipal(OpenIdClaims.ClientId, "my-client");

            var context = await HandleAsync(new CreateContent());

            Assert.Equal(RefToken.Client("my-client"), ((SquidexCommand)context.Command).Actor);
        }

        [Fact]
        public async Task Should_not_override_actor()
        {
            httpContext.User = CreatePrincipal(OpenIdClaims.ClientId, "my-client");

            var customActor = RefToken.User("me");

            var context = await HandleAsync(new CreateContent { Actor = customActor });

            Assert.Equal(customActor, ((SquidexCommand)context.Command).Actor);
        }

        private async Task<CommandContext> HandleAsync(ICommand command)
        {
            var commandContext = new CommandContext(command, A.Fake<ICommandBus>());

            await sut.HandleAsync(commandContext);

            return commandContext;
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
