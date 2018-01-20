// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
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

        public static async Task<List<ExternalProvider>> GetExternalProvidersAsync(this SignInManager<IUser> signInManager)
        {
            var externalSchemes = await signInManager.GetExternalAuthenticationSchemesAsync();
            var externalProviders =
                externalSchemes.Where(x => x.Name != OpenIdConnectDefaults.AuthenticationScheme)
                    .Select(x => new ExternalProvider(x.Name, x.DisplayName)).ToList();

            return externalProviders;
        }
    }
}
