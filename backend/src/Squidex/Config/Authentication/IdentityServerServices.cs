// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Web;

namespace Squidex.Config.Authentication
{
    public static class IdentityServerServices
    {
        public static AuthenticationBuilder AddSquidexIdentityServerAuthentication(this AuthenticationBuilder authBuilder, MyIdentityOptions identityOptions, IConfiguration config)
        {
            if (!string.IsNullOrWhiteSpace(identityOptions.AuthorityUrl))
            {
                var apiAuthorityUrl = identityOptions.AuthorityUrl;

                authBuilder.AddOpenIdConnect(options =>
                {
                    options.Authority = apiAuthorityUrl;
                    options.ClientId = Constants.InternalClientId;
                    options.ClientSecret = Constants.InternalClientSecret;
                    options.CallbackPath = "/signin-internal";
                    options.RequireHttpsMetadata = identityOptions.RequiresHttps;
                    options.SaveTokens = true;
                    options.Scope.Add(Constants.PermissionsScope);
                    options.Scope.Add(Constants.ProfileScope);
                    options.Scope.Add(Constants.RoleScope);
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                });
            }
            else
            {
                authBuilder.AddLocalApi(Constants.ApiSecurityScheme, options =>
                {
                    options.ExpectedScope = Constants.ApiScope;
                });
            }

            return authBuilder;
        }
    }
}
