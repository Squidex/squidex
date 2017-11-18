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
using Squidex.Shared.Users;

namespace Squidex.Areas.IdentityServer.Controllers
{
    public static class Extensions
    {
        public static async Task<ExternalLoginInfo> GetExternalLoginInfoWithDisplayNameAsync(this SignInManager<IUser> signInManager, string expectedXsrf = null)
        {
            var externalLogin = await signInManager.GetExternalLoginInfoAsync(expectedXsrf);

            externalLogin.ProviderDisplayName = externalLogin.Principal.FindFirst(ClaimTypes.Email).Value;

            return externalLogin;
        }
    }
}
