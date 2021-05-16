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
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Squidex.Config.Authentication
{
    public static class IdentityServerServices
    {
        public static AuthenticationBuilder AddSquidexIdentityServerAuthentication(this AuthenticationBuilder authBuilder, MyIdentityOptions identityOptions, IConfiguration config)
        {
            var useCustomAuthorityUrl = !string.IsNullOrWhiteSpace(identityOptions.AuthorityUrl);

            if (useCustomAuthorityUrl)
            {
                const string ExternalIdentityServerSchema = nameof(ExternalIdentityServerSchema);

                authBuilder.AddOpenIdConnect(ExternalIdentityServerSchema, options =>
                {
                    options.Authority = identityOptions.AuthorityUrl;
                    options.Scope.Add(Scopes.Email);
                    options.Scope.Add(Scopes.Profile);
                    options.Scope.Add(Constants.ScopePermissions);
                    options.Scope.Add(Constants.ScopeApi);
                });

                authBuilder.AddPolicyScheme(Constants.ApiSecurityScheme, Constants.ApiSecurityScheme, options =>
                {
                    options.ForwardDefaultSelector = context => ExternalIdentityServerSchema;
                });
            }
            else
            {
                authBuilder.AddPolicyScheme(Constants.ApiSecurityScheme, Constants.ApiSecurityScheme, options =>
                {
                    options.ForwardDefaultSelector = _ => OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                });
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
                    options.Scope.Add(Scopes.Email);
                    options.Scope.Add(Scopes.Profile);
                    options.Scope.Add(Constants.ScopePermissions);
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                });

            return authBuilder;
        }
    }
}
