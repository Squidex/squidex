// ==========================================================================
//  Extensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Squidex.Domain.Apps.Read.Users;

namespace Squidex.Controllers.UI
{
    public static class Extensions
    {
        public static Task<IdentityResult> UpdateAsync(this UserManager<IUser> userManager, IUser user, string email, string displayName)
        {
            user.UpdateEmail(email);
            user.UpdateDisplayName(displayName);

            return userManager.UpdateAsync(user);
        }

        public static async Task<ExternalLoginInfo> GetExternalLoginInfoWithDisplayNameAsync(this SignInManager<IUser> signInManager, string expectedXsrf = null)
        {
            var externalLogin = await signInManager.GetExternalLoginInfoAsync(expectedXsrf);

            externalLogin.ProviderDisplayName = externalLogin.Principal.FindFirst(ClaimTypes.Email).Value;

            return externalLogin;
        }
    }
}
