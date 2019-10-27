// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Users;

namespace Squidex.Config.Authentication
{
    public static class AuthenticationServices
    {
        public static void AddSquidexAuthentication(this IServiceCollection services, IConfiguration config)
        {
            var identityOptions = config.GetSection("identity").Get<MyIdentityOptions>();

            services.AddSingletonAs<DefaultXmlRepository>()
                .As<IXmlRepository>();

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
