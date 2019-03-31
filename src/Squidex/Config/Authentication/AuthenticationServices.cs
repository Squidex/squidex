// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Squidex.Config.Authentication
{
    public static class AuthenticationServices
    {
        public static void AddMyAuthentication(this IServiceCollection services, IConfiguration config)
        {
            var identityOptions = config.GetSection("identity").Get<MyIdentityOptions>();

            services.AddAuthentication()
                .AddMyCookie()
                .AddMyExternalGithubAuthentication(identityOptions)
                .AddMyExternalGoogleAuthentication(identityOptions)
                .AddMyExternalMicrosoftAuthentication(identityOptions)
                .AddMyExternalOdic(identityOptions)
                .AddMyIdentityServerAuthentication(identityOptions, config);
        }

        public static AuthenticationBuilder AddMyCookie(this AuthenticationBuilder builder)
        {
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = ".sq.auth";
            });

            return builder.AddCookie();
        }
    }
}
