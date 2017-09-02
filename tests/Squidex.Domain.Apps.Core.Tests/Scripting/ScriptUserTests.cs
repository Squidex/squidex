// ==========================================================================
//  ScriptUserTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Security.Claims;
using FluentAssertions;
using Squidex.Infrastructure.Security;
using Xunit;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public class ScriptUserTests
    {
        [Fact]
        public void Should_create_script_user_from_user_principal()
        {
            var identity = new ClaimsIdentity();

            identity.AddClaim(new Claim(OpenIdClaims.Subject, "1"));
            identity.AddClaim(new Claim(OpenIdClaims.Email, "hello@squidex.io"));
            identity.AddClaim(new Claim("claim1", "1a"));
            identity.AddClaim(new Claim("claim1", "1b"));
            identity.AddClaim(new Claim("claim2", "2a"));
            identity.AddClaim(new Claim("claim2", "2b"));

            var principal = new ClaimsPrincipal(new[] { identity });

            var scriptUser = ScriptUser.Create(principal);

            scriptUser.ShouldBeEquivalentTo(
                new ScriptUser
                {
                    Email = "hello@squidex.io",
                    Id = "1",
                    IsClient = false,
                    Claims = new Dictionary<string, string[]>
                    {
                        { "sub", new [] { "1" } },
                        { "claim1", new[] { "1a", "1b" } },
                        { "claim2", new[] { "2a", "2b" } },
                        { "email", new [] { "hello@squidex.io" } }
                    }
                });
        }

        [Fact]
        public void Should_create_script_user_from_client_principal()
        {
            var identity = new ClaimsIdentity();

            identity.AddClaim(new Claim(OpenIdClaims.ClientId, "1"));
            identity.AddClaim(new Claim("claim1", "1a"));
            identity.AddClaim(new Claim("claim1", "1b"));
            identity.AddClaim(new Claim("claim2", "2a"));
            identity.AddClaim(new Claim("claim2", "2b"));

            var principal = new ClaimsPrincipal(new[] { identity });

            var scriptUser = ScriptUser.Create(principal);

            scriptUser.ShouldBeEquivalentTo(
                new ScriptUser
                {
                    Id = "1",
                    IsClient = true,
                    Claims = new Dictionary<string, string[]>
                    {
                        { "client_id", new [] { "1" } } ,
                        { "claim1", new[] { "1a", "1b" } },
                        { "claim2", new[] { "2a", "2b" } }
                    }
                });
        }
    }
}
