// ==========================================================================
//  MicrosoftAuthenticationServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Squidex.Config.Identity
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
