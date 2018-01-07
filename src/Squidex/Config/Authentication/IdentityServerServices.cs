// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure;

namespace Squidex.Config.Authentication
{
    public static class IdentityServerServices
    {
        public static AuthenticationBuilder AddMyIdentityServerAuthentication(this AuthenticationBuilder authBuilder, MyIdentityOptions identityOptions, IConfiguration config)
        {
            var apiScope = Constants.ApiScope;

            var urlsOptions = config.GetSection("urls").Get<MyUrlsOptions>();

            if (!string.IsNullOrWhiteSpace(urlsOptions.BaseUrl))
            {
                string apiAuthorityUrl;

                if (!string.IsNullOrWhiteSpace(identityOptions.AuthorityUrl))
                {
                    apiAuthorityUrl = identityOptions.AuthorityUrl.BuildFullUrl(Constants.IdentityServerPrefix);
                }
                else
                {
                    apiAuthorityUrl = urlsOptions.BuildUrl(Constants.IdentityServerPrefix);
                }

                authBuilder.AddIdentityServerAuthentication(options =>
                {
                    options.Authority = apiAuthorityUrl;
                    options.ApiName = apiScope;
                    options.ApiSecret = null;
                    options.RequireHttpsMetadata = identityOptions.RequiresHttps;
                });

                authBuilder.AddOpenIdConnect(options =>
                {
                    options.Authority = apiAuthorityUrl;
                    options.ClientId = Constants.InternalClientId;
                    options.ClientSecret = Constants.InternalClientSecret;
                    options.RequireHttpsMetadata = identityOptions.RequiresHttps;
                    options.SaveTokens = true;
                    options.Scope.Add(Constants.ProfileScope);
                    options.Scope.Add(Constants.RoleScope);
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                });
            }

            return authBuilder;
        }
    }
}
