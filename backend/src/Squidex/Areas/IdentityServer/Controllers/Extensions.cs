// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;
using Squidex.Web;

namespace Squidex.Areas.IdentityServer.Controllers;

public static class Extensions
{
    public static async Task<ExternalLoginInfo> GetExternalLoginInfoWithDisplayNameAsync(this SignInManager<IdentityUser> signInManager, string? expectedXsrf = null)
    {
        var login = await signInManager.GetExternalLoginInfoAsync(expectedXsrf);

        if (login == null)
        {
            ThrowHelper.InvalidOperationException("Request from external provider cannot be handled.");
            return default!;
        }

        var email = login.Principal.GetEmail();

        if (string.IsNullOrWhiteSpace(email))
        {
            ThrowHelper.InvalidOperationException("External provider does not provide email claim.");
            return default!;
        }

        login.ProviderDisplayName = email;

        return login;
    }

    public static async Task<List<ExternalProvider>> GetExternalProvidersAsync(this SignInManager<IdentityUser> signInManager)
    {
        var externalSchemes = await signInManager.GetExternalAuthenticationSchemesAsync();

        var externalProviders = externalSchemes
            .Where(x => x.Name != OpenIdConnectDefaults.AuthenticationScheme)
            .Where(x => x.Name != Constants.ApiSecurityScheme)
            .Select(x => new ExternalProvider(x.Name, x.DisplayName ?? x.Name))
            .ToList();

        return externalProviders;
    }
}
