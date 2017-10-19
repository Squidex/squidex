// ==========================================================================
//  GoogleAuthenticationServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Squidex.Config.Identity
{
    public static class GoogleAuthenticationServices
    {
        public static AuthenticationBuilder AddMyGoogleAuthentication(this AuthenticationBuilder authBuilder, MyIdentityOptions identityOptions)
        {
            if (identityOptions.IsGoogleAuthConfigured())
            {
                authBuilder.AddGoogle(options =>
                {
                    options.ClientId = identityOptions.GoogleClient;
                    options.ClientSecret = identityOptions.GoogleSecret;
                    options.Events = new GoogleHandler();
                });
            }

            return authBuilder;
        }
    }
}
