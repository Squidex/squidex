// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Shared.Identity;

namespace Squidex.Domain.Users;

public sealed class UserClaimsPrincipalFactoryWithEmail : UserClaimsPrincipalFactory<IdentityUser, IdentityRole>
{
    private const string AdministratorRole = "ADMINISTRATOR";

    public UserClaimsPrincipalFactoryWithEmail(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
    }

    public override async Task<ClaimsPrincipal> CreateAsync(IdentityUser user)
    {
        var userPrincipal = await base.CreateAsync(user);
        var userIdentity = userPrincipal.Identities.First();

        var email = await UserManager.GetEmailAsync(user);

        if (email != null)
        {
            userIdentity.AddClaim(new Claim(OpenIdClaims.Email, email));
        }

        if (await UserManager.IsInRoleAsync(user, AdministratorRole))
        {
            userIdentity.AddClaim(new Claim(SquidexClaimTypes.Permissions, PermissionIds.Admin));
        }

        return userPrincipal;
    }
}
