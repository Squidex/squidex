// ==========================================================================
//  AuthenticationServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

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
                .AddMyGoogleAuthentication(identityOptions)
                .AddMyMicrosoftAuthentication(identityOptions)
                .AddMyIdentityServerAuthentication(identityOptions, config)
                .AddCookie();
        }
    }
}
