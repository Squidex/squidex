// ==========================================================================
//  JintUserTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Security.Claims;
using Jint;
using Squidex.Infrastructure.Security;
using Xunit;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public class JintUserTests
    {
        [Fact]
        public void Should_set_user_id_from_client_id()
        {
            var identity = new ClaimsIdentity();

            identity.AddClaim(new Claim(OpenIdClaims.ClientId, "1"));

            Assert.Equal("1", GetValue(identity, "user.id"));
            Assert.Equal(true, GetValue(identity, "user.isClient"));
        }

        [Fact]
        public void Should_set_user_id_from_subject_id()
        {
            var identity = new ClaimsIdentity();

            identity.AddClaim(new Claim(OpenIdClaims.Subject, "2"));

            Assert.Equal("2", GetValue(identity, "user.id"));
            Assert.Equal(false, GetValue(identity, "user.isClient"));
        }

        [Fact]
        public void Should_set_email_from_claim()
        {
            var identity = new ClaimsIdentity();

            identity.AddClaim(new Claim(OpenIdClaims.Email, "hello@squidex.io"));

            Assert.Equal("hello@squidex.io", GetValue(identity, "user.email"));
        }

        [Fact]
        public void Should_set_claims()
        {
            var identity = new ClaimsIdentity();

            identity.AddClaim(new Claim("claim1", "1a"));
            identity.AddClaim(new Claim("claim1", "1b"));
            identity.AddClaim(new Claim("claim2", "2a"));
            identity.AddClaim(new Claim("claim2", "2b"));

            Assert.Equal(new[] { "1a", "1b" }, GetValue(identity, "user.claims.claim1"));
            Assert.Equal(new[] { "2a", "2b" }, GetValue(identity, "user.claims.claim2"));
        }

        private static object GetValue(ClaimsIdentity identity, string script)
        {
            var engine = new Engine();

            engine.SetValue("user", new JintUser(engine, new ClaimsPrincipal(new[] { identity })));

            return engine.Execute(script).GetCompletionValue().ToObject();
        }
    }
}
