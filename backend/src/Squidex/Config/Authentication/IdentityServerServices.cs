// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Validation.AspNetCore;
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
                /*
                authBuilder.AddOpenIdConnect(options =>
                {
                    options.Authority = identityOptions.AuthorityUrl;
                    options.ApiName = Constants.ApiScope;
                    options.ApiSecret = null;
                    options.RequireHttpsMetadata = identityOptions.RequiresHttps;
                    options.SupportedTokens = SupportedTokens.Jwt;
                });*/
            }
            else
            {
                /*
                authBuilder.AddLocalApi();

                authBuilder.Services.AddOptions<LocalApiAuthenticationOptions>(IdentityServerConstants.LocalApi.PolicyName)
                    .Configure<IUrlGenerator>((options, urlGenerator) =>
                    {
                        options.ClaimsIssuer = urlGenerator.BuildUrl(Constants.IdentityServerPrefix, false);

                        options.ExpectedScope = Constants.ApiScope;
                    });*/
            }

            authBuilder.AddOpenIdConnect();

            authBuilder.Services.AddOptions<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme)
                .Configure<IUrlGenerator>((options, urlGenerator) =>
                {
                    if (!string.IsNullOrWhiteSpace(identityOptions.AuthorityUrl))
                    {
                        options.Authority = identityOptions.AuthorityUrl;
                    }
                    else
                    {
                        options.Authority = urlGenerator.BuildUrl(Constants.PrefixIdentityServer, false);
                    }

                    options.ClientId = Constants.ClientInternalId;
                    options.ClientSecret = Constants.ClientInternalSecret;
                    options.CallbackPath = "/signin-internal";
                    options.RequireHttpsMetadata = identityOptions.RequiresHttps;
                    options.SaveTokens = true;
                    options.Scope.Add(Constants.ScopePermissions);
                    options.Scope.Add(Constants.ScopeProfile);
                    options.Scope.Add(Constants.ScopeRole);
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                });

            authBuilder.AddPolicyScheme(Constants.ApiSecurityScheme, Constants.ApiSecurityScheme, options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    if (useCustomAuthorityUrl)
                    {
                        return OpenIdConnectDefaults.AuthenticationScheme;
                    }

                    return OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                };
            });

            return authBuilder;
        }
    }
}
