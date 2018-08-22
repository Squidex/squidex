// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
namespace Squidex.Config.Authentication
{
    public static class AuthenticationServices
    {
        public static void AddMyAuthentication(this IServiceCollection services, IConfiguration config)
        {
            var identityOptions = config.GetSection("identity").Get<MyIdentityOptions>();
            var urlsOptions = config.GetSection("urls").Get<MyUrlsOptions>();

            services.AddOidcStateDataFormatterCache();

            services
            .AddAuthentication()
            .AddOpenIdConnect("oidc", options =>
            {
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.Authority = "http://localhost:5000";
                options.ClientId = "my-client";
                options.ClientSecret = Constants.InternalClientSecret;
                options.RequireHttpsMetadata = identityOptions.RequiresHttps;
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.Scope.Add(Constants.ProfileScope);
                options.Scope.Add(Constants.RoleScope);
                options.Scope.Add("email");
                options.ResponseType = OpenIdConnectResponseType.IdToken;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role,
                };
            })
            .AddCookie()
            .AddMyGoogleAuthentication(identityOptions)
            .AddMyMicrosoftAuthentication(identityOptions)
            .AddMyIdentityServerAuthentication(identityOptions, config)
            ;
        }
    }
}
