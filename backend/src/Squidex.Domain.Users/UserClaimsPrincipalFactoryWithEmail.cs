// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Shared.Identity;

namespace Squidex.Domain.Users
{
    public sealed class UserClaimsPrincipalFactoryWithEmail : UserClaimsPrincipalFactory<IdentityUser, IdentityRole>
    {
        private const string AdministratorRole = "ADMINISTRATOR";

        public UserClaimsPrincipalFactoryWithEmail(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        public override async Task<ClaimsPrincipal> CreateAsync(IdentityUser user)
        {
            var principal = await base.CreateAsync(user);

            var identity = principal.Identities.First();

            identity.AddClaim(new Claim(OpenIdClaims.Email, await UserManager.GetEmailAsync(user)));

            if (await UserManager.IsInRoleAsync(user, AdministratorRole))
            {
                identity.AddClaim(new Claim(SquidexClaimTypes.Permissions, Permissions.Admin));
            }

            return principal;
        }
    }
}
