// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using IdentityServer4;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Web;

namespace Squidex.Config.Authentication
{
    public static class IdentityServerServices
    {
        public static AuthenticationBuilder AddSquidexIdentityServerAuthentication(this AuthenticationBuilder authBuilder, MyIdentityOptions identityOptions, IConfiguration config)
        {
            var apiAuthorityUrl = identityOptions.AuthorityUrl;

            var useCustomAuthorityUrl = !string.IsNullOrWhiteSpace(apiAuthorityUrl);

            if (!useCustomAuthorityUrl)
            {
                var urlsOptions = config.GetSection("urls").Get<UrlsOptions>();

                apiAuthorityUrl = urlsOptions.BuildUrl(Constants.IdentityServerPrefix);
            }

            var apiScope = Constants.ApiScope;

            if (useCustomAuthorityUrl)
            {
                authBuilder.AddIdentityServerAuthentication(options =>
                {
                    options.Authority = apiAuthorityUrl;
                    options.ApiName = apiScope;
                    options.ApiSecret = null;
                    options.RequireHttpsMetadata = identityOptions.RequiresHttps;
                    options.SupportedTokens = SupportedTokens.Jwt;
                });
            }
            else
            {
                var urlsOptions = config.GetSection("urls").Get<UrlsOptions>();

                authBuilder.AddLocalApi(options =>
                {
                    options.ClaimsIssuer = urlsOptions.BuildUrl("/identity-server", false);

                    options.ExpectedScope = apiScope;
                });
            }

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

            authBuilder.AddPolicyScheme(Constants.ApiSecurityScheme, Constants.ApiSecurityScheme, options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    if (useCustomAuthorityUrl)
                    {
                        return IdentityServerAuthenticationDefaults.AuthenticationScheme;
                    }

                    return IdentityServerConstants.LocalApi.PolicyName;
                };
            });

            return authBuilder;
        }
    }
}
