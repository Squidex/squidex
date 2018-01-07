// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Security.Claims;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.Json
{
    public class ClaimsPrincipalConverterTests
    {
        [Fact]
        public void Should_serialize_and_deserialize()
        {
            var value = new ClaimsPrincipal(
                new[]
                {
                    new ClaimsIdentity(
                        new[]
                        {
                            new Claim("email", "me@email.de"),
                            new Claim("username", "me@email.de")
                        },
                        "Cookie"),
                    new ClaimsIdentity(
                        new[]
                        {
                            new Claim("user_id", "12345"),
                            new Claim("login", "me")
                        },
                        "Google")
                });

            var result = value.SerializeAndDeserializeAndReturn(new ClaimsPrincipalConverter());

            Assert.Equal(value.Identities.ElementAt(0).AuthenticationType, result.Identities.ElementAt(0).AuthenticationType);
            Assert.Equal(value.Identities.ElementAt(1).AuthenticationType, result.Identities.ElementAt(1).AuthenticationType);
        }

        [Fact]
        public void Should_serialize_and_deserialize_null_principal()
        {
            ClaimsPrincipal value = null;

            value.SerializeAndDeserialize(new ClaimsPrincipalConverter());
        }
    }
}
