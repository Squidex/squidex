// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Pipeline.CommandMiddlewares;
using Xunit;

namespace Squidex.Tests.Pipeline.CommandMiddlewares
{
    public class EnrichWithActorCommandMiddlewareTests
    {
        private readonly IHttpContextAccessor httpContextAccessor = A.Fake<IHttpContextAccessor>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly CreateContent command = new CreateContent { Actor = null };

        [Fact]
        public async Task HandleAsync_should_throw_security_exception()
        {
            var context = new CommandContext(command, commandBus);
            var sut = SetupSystem(null, out string claimValue);

            await Assert.ThrowsAsync<SecurityException>(() =>
            {
                return sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task HandleAsync_should_find_actor_from_subject()
        {
            var context = new CommandContext(command, commandBus);
            var sut = SetupSystem("subject", out string claimValue);

            await sut.HandleAsync(context);

            Assert.Equal(claimValue, command.Actor.Identifier);
        }

        [Fact]
        public async Task HandleAsync_should_find_actor_from_client()
        {
            var context = new CommandContext(command, commandBus);
            var sut = SetupSystem("client", out string claimValue);

            await sut.HandleAsync(context);

            Assert.Equal(claimValue, command.Actor.Identifier);
        }

        private EnrichWithActorCommandMiddleware SetupSystem(string refTokenType, out string claimValue)
        {
            Claim actorClaim;
            claimValue = Guid.NewGuid().ToString();
            var user = new ClaimsPrincipal();
            var claimsIdentity = new ClaimsIdentity();
            switch (refTokenType)
            {
                case "subject":
                    actorClaim = new Claim(OpenIdClaims.Subject, claimValue);
                    claimsIdentity.AddClaim(actorClaim);
                    break;
                case "client":
                    actorClaim = new Claim(OpenIdClaims.ClientId, claimValue);
                    claimsIdentity.AddClaim(actorClaim);
                    break;
            }

            user.AddIdentity(claimsIdentity);
            A.CallTo(() => httpContextAccessor.HttpContext.User).Returns(user);
            return new EnrichWithActorCommandMiddleware(httpContextAccessor);
        }
    }
}
