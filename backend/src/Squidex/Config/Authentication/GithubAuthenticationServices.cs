// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;

namespace Squidex.Config.Authentication;

public static class GithubAuthenticationServices
{
    public static AuthenticationBuilder AddSquidexExternalGithubAuthentication(this AuthenticationBuilder authBuilder, MyIdentityOptions identityOptions)
    {
        if (identityOptions.IsGithubAuthConfigured())
        {
            authBuilder.AddGitHub(options =>
            {
                options.ClientId = identityOptions.GithubClient;
                options.ClientSecret = identityOptions.GithubSecret;
                options.Events = new GithubHandler();
                options.Scope.Add("user:email");
            });
        }

        return authBuilder;
    }
}
