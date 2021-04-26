// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;

namespace Squidex.Config.Authentication
{
    public static class OidcServices
    {
        public static AuthenticationBuilder AddSquidexExternalOdic(this AuthenticationBuilder authBuilder, MyIdentityOptions identityOptions)
        {
            if (identityOptions.IsOidcConfigured())
            {
                var displayName = !string.IsNullOrWhiteSpace(identityOptions.OidcName) ? identityOptions.OidcName : OpenIdConnectDefaults.DisplayName;

                authBuilder.AddOpenIdConnect("ExternalOidc", displayName, options =>
                {
                    options.Authority = identityOptions.OidcAuthority;
                    options.ClientId = identityOptions.OidcClient;
                    options.ClientSecret = identityOptions.OidcSecret;
                    options.RequireHttpsMetadata = identityOptions.RequiresHttps;
                    options.Events = new OidcHandler(identityOptions);

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
                        foreach (var scope in identityOptions.OidcScopes)
                        {
                            options.Scope.Add(scope);
                        }
                    }
                });
            }

            return authBuilder;
        }
    }
}
