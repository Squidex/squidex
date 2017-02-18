// ==========================================================================
//  IdentityUsage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.MongoDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Squidex.Core.Identity;
using Squidex.Infrastructure.Security;

// ReSharper disable InvertIf

namespace Squidex.Config.Identity
{
    public static class IdentityUsage
    {
        public static IApplicationBuilder UseMyIdentity(this IApplicationBuilder app)
        {
            app.UseIdentity();

            return app;
        }

        public static IApplicationBuilder UseMyIdentityServer(this IApplicationBuilder app)
        {
             app.UseIdentityServer();

            return app;
        }

        public static IApplicationBuilder UseMyDefaultUser(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetService<IOptions<MyIdentityOptions>>().Value;

            var username = options.DefaultUsername;
            var userManager = app.ApplicationServices.GetService<UserManager<IdentityUser>>();

            if (!string.IsNullOrWhiteSpace(options.DefaultUsername) &&
                !string.IsNullOrWhiteSpace(options.DefaultPassword))
            {
                Task.Run(async () =>
                {
                    if (userManager.SupportsQueryableUsers && !userManager.Users.Any())
                    {
                        var user = new IdentityUser { UserName = username, Email = username, EmailConfirmed = true };

                        await userManager.CreateAsync(user, options.DefaultPassword);
                    }
                }).Wait();
            }

            return app;
        }

        public static IApplicationBuilder UseMyGoogleAuthentication(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetService<IOptions<MyIdentityOptions>>().Value;

            if (!string.IsNullOrWhiteSpace(options.GoogleClient) &&
                !string.IsNullOrWhiteSpace(options.GoogleSecret))
            {
                var googleOptions =
                    new GoogleOptions
                    {
                        Events = new GoogleHandler(),
                        ClientId = options.GoogleClient,
                        ClientSecret = options.GoogleSecret
                    };

                app.UseGoogleAuthentication(googleOptions);
            }

            return app;
        }

        public static IApplicationBuilder UseMyApiProtection(this IApplicationBuilder app)
        {
            const string apiScope = Constants.ApiScope;

            var urlsOptions = app.ApplicationServices.GetService<IOptions<MyUrlsOptions>>().Value;

            if (!string.IsNullOrWhiteSpace(urlsOptions.BaseUrl))
            {
                var apiAuthorityUrl = urlsOptions.BuildUrl(Constants.IdentityPrefix);

                var identityOptions = app.ApplicationServices.GetService<IOptions<MyIdentityOptions>>().Value;

                app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
                {
                    Authority = apiAuthorityUrl,
                    ApiName = apiScope,
                    ApiSecret = null,
                    RequireHttpsMetadata = identityOptions.RequiresHttps
                });
            }

            return app;
        }

        private class RetrieveClaimsHandler : OAuthEvents
        {
            public override Task CreatingTicket(OAuthCreatingTicketContext context)
            {
                var displayNameClaim = context.Identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
                if (displayNameClaim != null)
                {
                    context.Identity.AddClaim(new Claim(SquidexClaimTypes.SquidexDisplayName, displayNameClaim.Value));
                }

                return base.CreatingTicket(context);
            }
        }

        private sealed class GoogleHandler : RetrieveClaimsHandler
        {
            private static readonly HttpClient HttpClient = new HttpClient();

            public override Task RedirectToAuthorizationEndpoint(OAuthRedirectToAuthorizationContext context)
            {
                context.Response.Redirect(context.RedirectUri + "&prompt=select_account");

                return Task.FromResult(true);
            }

            public override async Task CreatingTicket(OAuthCreatingTicketContext context)
            {
                if (!string.IsNullOrWhiteSpace(context.AccessToken))
                {
                    var apiRequestUri = new Uri($"https://www.googleapis.com/oauth2/v2/userinfo?access_token={context.AccessToken}");

                    var jsonReponseString = await HttpClient.GetStringAsync(apiRequestUri);
                    var jsonResponse = JToken.Parse(jsonReponseString);

                    var pictureUrl = jsonResponse["picture"]?.Value<string>();

                    if (!string.IsNullOrWhiteSpace(pictureUrl))
                    {
                        context.Identity.AddClaim(new Claim(SquidexClaimTypes.SquidexPictureUrl, pictureUrl));
                    }
                }

                await base.CreatingTicket(context);
            }
        }
    }
}
