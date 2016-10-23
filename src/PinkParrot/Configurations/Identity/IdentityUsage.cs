// ==========================================================================
//  IdentityUsage.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.MongoDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// ReSharper disable InvertIf

namespace PinkParrot.Configurations.Identity
{
    public static class IdentityUsage
    {
        public static IApplicationBuilder UseMyIdentity(this IApplicationBuilder app)
        {
            app.UseIdentity();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = "Cookies"
            });

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
                        ClientId = options.GoogleClient,
                        ClientSecret = options.GoogleSecret
                    };

                app.UseGoogleAuthentication(googleOptions);
            }

            return app;
        }

        public static IApplicationBuilder UseMyApiProtection(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetService<IOptions<MyIdentityOptions>>().Value;

            if (!string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                app.Map("/api", api =>
                {
                    api.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
                    {
                        Authority = options.BaseUrl,
                        ScopeName = "api",
                        RequireHttpsMetadata = options.RequiresHttps
                    });
                });
            }

            return app;
        }
    }
}
