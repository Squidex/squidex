// ==========================================================================
//  AuthenticationServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure;

namespace Squidex.Config.Authentication
{
    public static class AuthenticationServices
    {
        public static void AddMyAuthentication(this IServiceCollection services, IConfiguration config)
        {
            var identityOptions = config.GetSection("identity").Get<MyIdentityOptions>();

            services.AddAuthentication()
                .AddCookie()
                .AddMyGoogleAuthentication(identityOptions)
                .AddMyMicrosoftAuthentication(identityOptions)
                .AddMyApiProtection(identityOptions, config);
        }

        public static AuthenticationBuilder AddMyApiProtection(this AuthenticationBuilder authBuilder, MyIdentityOptions identityOptions, IConfiguration config)
        {
            var apiScope = Constants.ApiScope;

            var urlsOptions = config.GetSection("urls").Get<MyUrlsOptions>();

            if (!string.IsNullOrWhiteSpace(urlsOptions.BaseUrl))
            {
                string apiAuthorityUrl;

                if (!string.IsNullOrWhiteSpace(identityOptions.AuthorityUrl))
                {
                    apiAuthorityUrl = identityOptions.AuthorityUrl.BuildFullUrl(Constants.IdentityPrefix);
                }
                else
                {
                    apiAuthorityUrl = urlsOptions.BuildUrl(Constants.IdentityPrefix);
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
                    options.ClientId = Constants.InternalClientId;
                    options.ClientSecret = Constants.InternalClientSecret;
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.Authority = apiAuthorityUrl;
                    options.RequireHttpsMetadata = identityOptions.RequiresHttps;
                    options.Scope.Add(Constants.RoleScope);
                    options.SaveTokens = true;
                });
            }

            return authBuilder;
        }
    }
}
