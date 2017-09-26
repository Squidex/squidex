// ==========================================================================
//  UserClaimsPrincipalFactoryWithEmail.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure.Security;
using Squidex.Shared.Users;

namespace Squidex.Domain.Users
{
    public sealed class UserClaimsPrincipalFactoryWithEmail : UserClaimsPrincipalFactory<IUser, IRole>
    {
        public UserClaimsPrincipalFactoryWithEmail(UserManager<IUser> userManager, RoleManager<IRole> roleManager, IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
        {
        }

        public override async Task<ClaimsPrincipal> CreateAsync(IUser user)
        {
            var principal = await base.CreateAsync(user);

            principal.Identities.First().AddClaim(new Claim(OpenIdClaims.Email, await UserManager.GetEmailAsync(user)));

            return principal;
        }
    }
}
