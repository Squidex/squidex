﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Squidex.Config.Authentication
{
    public static class GoogleAuthenticationServices
    {
        public static AuthenticationBuilder AddSquidexExternalGoogleAuthentication(this AuthenticationBuilder authBuilder, MyIdentityOptions identityOptions)
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
