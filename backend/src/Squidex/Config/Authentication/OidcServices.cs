﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Squidex.Infrastructure;
using Squidex.Web;

namespace Squidex.Config.Authentication;

public static class OidcServices
{
    public static AuthenticationBuilder AddSquidexExternalOdic(this AuthenticationBuilder authBuilder, MyIdentityOptions identityOptions)
    {
        if (identityOptions.IsOidcConfigured())
        {
            var displayName = !string.IsNullOrWhiteSpace(identityOptions.OidcName) ?
                identityOptions.OidcName :
                OpenIdConnectDefaults.DisplayName;

            authBuilder.AddOpenIdConnect(Constants.ExternalScheme, displayName, options =>
            {
                if (identityOptions.OidcDisableProfileScope)
                {
                    options.Scope.Clear();
                    options.Scope.Add(OpenIddict.Abstractions.OpenIddictConstants.Scopes.OpenId);
                }

                options.Events = new OidcHandler(identityOptions);
                options.Authority = identityOptions.OidcAuthority;
                options.Prompt = identityOptions.OidcPrompt;
                options.ClientId = identityOptions.OidcClient;
                options.ClientSecret = identityOptions.OidcSecret;
                options.RequireHttpsMetadata = identityOptions.RequiresHttps;

                if (!string.IsNullOrEmpty(identityOptions.OidcMetadataAddress))
                {
                    options.MetadataAddress = identityOptions.OidcMetadataAddress;
                }

                if (!string.IsNullOrEmpty(identityOptions.OidcResponseType))
                {
                    options.ResponseType = identityOptions.OidcResponseType;
                }

                options.GetClaimsFromUserInfoEndpoint = identityOptions.OidcGetClaimsFromUserInfoEndpoint;

                if (identityOptions.OidcScopes != null)
                {
                    options.Scope.AddRange(identityOptions.OidcScopes);
                }
            });
        }

        return authBuilder;
    }
}
