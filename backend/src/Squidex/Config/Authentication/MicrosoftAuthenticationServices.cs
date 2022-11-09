// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;

namespace Squidex.Config.Authentication;

public static class MicrosoftAuthenticationServices
{
    public static AuthenticationBuilder AddSquidexExternalMicrosoftAuthentication(this AuthenticationBuilder authBuilder, MyIdentityOptions identityOptions)
    {
        if (identityOptions.IsMicrosoftAuthConfigured())
        {
            authBuilder.AddMicrosoftAccount(options =>
            {
                options.ClientId = identityOptions.MicrosoftClient;
                options.ClientSecret = identityOptions.MicrosoftSecret;
                options.Events = new MicrosoftHandler();

                var tenantId = identityOptions.MicrosoftTenant;

                if (!string.IsNullOrEmpty(tenantId))
                {
                    var resource = "https://graph.microsoft.com";

                    options.AuthorizationEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/authorize?resource={resource}";
                    options.TokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/token?resource={resource}";
                }
            });
        }

        return authBuilder;
    }
}
