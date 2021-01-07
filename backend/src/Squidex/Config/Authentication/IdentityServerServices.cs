// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using IdentityServer4;
using IdentityServer4.AccessTokenValidation;
using IdentityServer4.Hosting.LocalApiAuthentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Hosting;
using Squidex.Web;

namespace Squidex.Config.Authentication
{
    public static class IdentityServerServices
    {
        public static AuthenticationBuilder AddSquidexIdentityServerAuthentication(this AuthenticationBuilder authBuilder, MyIdentityOptions identityOptions, IConfiguration config)
        {
            var useCustomAuthorityUrl = !string.IsNullOrWhiteSpace(identityOptions.AuthorityUrl);

            if (useCustomAuthorityUrl)
            {
                authBuilder.AddIdentityServerAuthentication(options =>
                {
                    options.Authority = identityOptions.AuthorityUrl;
                    options.ApiName = Constants.ApiScope;
                    options.ApiSecret = null;
                    options.RequireHttpsMetadata = identityOptions.RequiresHttps;
                    options.SupportedTokens = SupportedTokens.Jwt;
                });
            }
            else
            {
                authBuilder.AddLocalApi();

                authBuilder.Services.Configure<LocalApiAuthenticationOptions>((c, options) =>
                {
                    options.ClaimsIssuer = GetAuthorityUrl(c);

                    options.ExpectedScope = Constants.ApiScope;
                });
            }

            authBuilder.Services.AddSingleton<IPostConfigureOptions<OpenIdConnectOptions>>(c => new PostConfigureOptions<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                if (!string.IsNullOrWhiteSpace(identityOptions.AuthorityUrl))
                {
                    options.Authority = identityOptions.AuthorityUrl;
                }
                else
                {
                    options.Authority = GetAuthorityUrl(c);
                }

                options.ClientId = Constants.InternalClientId;
                options.ClientSecret = Constants.InternalClientSecret;
                options.CallbackPath = "/signin-internal";
                options.RequireHttpsMetadata = identityOptions.RequiresHttps;
                options.SaveTokens = true;
                options.Scope.Add(Constants.PermissionsScope);
                options.Scope.Add(Constants.ProfileScope);
                options.Scope.Add(Constants.RoleScope);
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }));

            authBuilder.AddOpenIdConnect();

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

        private static string GetAuthorityUrl(IServiceProvider services)
        {
            var urlGenerator = services.GetRequiredService<IUrlGenerator>();

            return urlGenerator.BuildUrl(Constants.IdentityServerPrefix, false);
        }
    }
}
