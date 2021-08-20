// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Squidex.Config.Authentication
{
    public static class AuthenticationServices
    {
        public static void AddSquidexAuthentication(this IServiceCollection services, IConfiguration config)
        {
            var identityOptions = config.GetSection("identity").Get<MyIdentityOptions>() ?? new ();

            services.AddAuthentication()
                .AddSquidexCookies()
                .AddSquidexExternalGithubAuthentication(identityOptions)
                .AddSquidexExternalGoogleAuthentication(identityOptions)
                .AddSquidexExternalMicrosoftAuthentication(identityOptions)
                .AddSquidexExternalOdic(identityOptions)
                .AddSquidexIdentityServerAuthentication(identityOptions, config);
        }

        public static AuthenticationBuilder AddSquidexCookies(this AuthenticationBuilder builder)
        {
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = ".sq.auth";
            });

            return builder.AddCookie();
        }
    }
}
