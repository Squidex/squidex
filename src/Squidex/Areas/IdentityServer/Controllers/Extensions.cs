﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;

namespace Squidex.Areas.IdentityServer.Controllers
{
    public static class Extensions
    {
        public static async Task<ExternalLoginInfo> GetExternalLoginInfoWithDisplayNameAsync(this SignInManager<IdentityUser> signInManager, string? expectedXsrf = null)
        {
            var externalLogin = await signInManager.GetExternalLoginInfoAsync(expectedXsrf);

            var email = externalLogin.Principal.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new InvalidOperationException("External provider does not provide email claim.");
            }

            externalLogin.ProviderDisplayName = email;

            return externalLogin;
        }

        public static async Task<List<ExternalProvider>> GetExternalProvidersAsync(this SignInManager<IdentityUser> signInManager)
        {
            var externalSchemes = await signInManager.GetExternalAuthenticationSchemesAsync();

            var externalProviders =
                externalSchemes.Where(x => x.Name != OpenIdConnectDefaults.AuthenticationScheme)
                    .Select(x => new ExternalProvider(x.Name, x.DisplayName)).ToList();

            return externalProviders;
        }
    }
}
