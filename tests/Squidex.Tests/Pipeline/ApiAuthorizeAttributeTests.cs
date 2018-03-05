// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using IdentityServer4.AccessTokenValidation;
using Squidex.Pipeline;
using Squidex.Shared.Identity;
using Xunit;

namespace Squidex.Tests.Pipeline
{
    public class ApiAuthorizeAttributeTests
    {
        private ApiAuthorizeAttribute sut = new ApiAuthorizeAttribute();

        [Fact]
        public void AuthenticationSchemes_should_be_default()
        {
            Assert.Equal(IdentityServerAuthenticationDefaults.AuthenticationScheme, sut.AuthenticationSchemes);
        }

        [Fact]
        public void MustBeAdmin_Test()
        {
            sut = new MustBeAdministratorAttribute();
            Assert.Equal(SquidexRoles.Administrator, sut.Roles);
        }
    }
}
