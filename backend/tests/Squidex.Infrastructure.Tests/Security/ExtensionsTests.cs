// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Security.Claims;
using Xunit;

namespace Squidex.Infrastructure.Security
{
    public class ExtensionsTests
    {
        [Fact]
        public void Should_retrieve_subject()
        {
            TestClaimExtension(OpenIdClaims.Subject, x => x.OpenIdSubject());
        }

        [Fact]
        public void Should_retrieve_client_id()
        {
            TestClaimExtension(OpenIdClaims.ClientId, x => x.OpenIdClientId());
        }

        [Fact]
        public void Should_retrieve_preferred_user_name()
        {
            TestClaimExtension(OpenIdClaims.PreferredUserName, x => x.OpenIdPreferredUserName());
        }

        [Fact]
        public void Should_retrieve_name()
        {
            TestClaimExtension(OpenIdClaims.Name, x => x.OpenIdName());
        }

        [Fact]
        public void Should_retrieve_email()
        {
            TestClaimExtension(OpenIdClaims.Email, x => x.OpenIdEmail());
        }

        private static void TestClaimExtension(string claimType, Func<ClaimsPrincipal, string?> getter)
        {
            var claimValue = Guid.NewGuid().ToString();

            var claimsIdentity = new ClaimsIdentity();
            var claimsPrincipal = new ClaimsPrincipal();

            claimsIdentity.AddClaim(new Claim(claimType, claimValue));

            Assert.Null(getter(claimsPrincipal));

            claimsPrincipal.AddIdentity(claimsIdentity);

            Assert.Equal(claimValue, getter(claimsPrincipal));
        }
    }
}
