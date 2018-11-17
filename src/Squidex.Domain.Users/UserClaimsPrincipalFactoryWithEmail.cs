// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure.Security;

namespace Squidex.Domain.Users
{
    public sealed class UserClaimsPrincipalFactoryWithEmail : UserClaimsPrincipalFactory<IdentityUser, IdentityRole>
    {
        public UserClaimsPrincipalFactoryWithEmail(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        public override async Task<ClaimsPrincipal> CreateAsync(IdentityUser user)
        {
            var principal = await base.CreateAsync(user);

            principal.Identities.First().AddClaim(new Claim(OpenIdClaims.Email, await UserManager.GetEmailAsync(user)));

            return principal;
        }
    }
}
