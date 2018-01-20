// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Squidex.Config.Authentication
{
    public static class MicrosoftAuthenticationServices
    {
        public static AuthenticationBuilder AddMyMicrosoftAuthentication(this AuthenticationBuilder authBuilder, MyIdentityOptions identityOptions)
        {
            if (identityOptions.IsMicrosoftAuthConfigured())
            {
                authBuilder.AddMicrosoftAccount(options =>
                {
                    options.ClientId = identityOptions.MicrosoftClient;
                    options.ClientSecret = identityOptions.MicrosoftSecret;
                    options.Events = new MicrosoftHandler();
                });
            }

            return authBuilder;
        }
    }
}
