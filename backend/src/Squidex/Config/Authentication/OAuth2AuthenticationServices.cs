// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using System.Net.Http;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Squidex.Config.Authentication
{
    public static class OAuth2AuthenticationServices
    {
        public static AuthenticationBuilder AddSquidexExternalOAuth2Authentication(this AuthenticationBuilder authBuilder, MyIdentityOptions identityOptions)
        {
            if (identityOptions.IsOAuthConfigured())
            {
                HttpClientHandler hch = new HttpClientHandler();
                hch.ServerCertificateCustomValidationCallback = delegate { return true; };

                HttpClient httpClient = new HttpClient(new LoggingHandler(hch));
                httpClient.SetBasicAuthenticationOAuth(identityOptions.OAuthClientId, identityOptions.OAuthSecret);
                authBuilder.AddOAuth(identityOptions.OAuthSchemaName, identityOptions.OAuthDisplayName, options =>
                {
                    options.ClientId = identityOptions.OAuthClientId;
                    options.ClientSecret = identityOptions.OAuthSecret;
                    options.TokenEndpoint = identityOptions.OAuthTokenURI;
                    options.UserInformationEndpoint = identityOptions.OAuthUserInfoURI;
                    options.AuthorizationEndpoint = identityOptions.OAuthAuthorizationURI;
                    options.CallbackPath = "/OAuth2Reply";
                    options.Scope.Add("read");
                    options.Events = new OAuth2Handler();
                    options.BackchannelHttpHandler = hch;
                    options.Backchannel = httpClient;
                    options.SaveTokens = true;
                });
            }

            return authBuilder;
        }
    }
}
