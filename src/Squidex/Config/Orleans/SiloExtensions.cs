// ==========================================================================
//  SiloExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Reflection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Orleans;
using Orleans.Hosting;
using Orleans.Runtime.Configuration;
using Squidex.Shared.Identity;

namespace Squidex.Config.Orleans
{
    public static class SiloExtensions
    {
        public static ISiloHostBuilder UseContentRoot(this ISiloHostBuilder builder, string path)
        {
            builder.ConfigureAppConfiguration(config =>
            {
                config.SetBasePath(path);
            });

            return builder;
        }

        public static ClusterConfiguration WithJsonSerializer(this ClusterConfiguration config)
        {
            config.Globals.SerializationProviders.Add(typeof(CustomJsonSerializer).GetTypeInfo());

            return config;
        }

        public static ClusterConfiguration WithDashboard(this ClusterConfiguration config)
        {
            config.RegisterDashboard();

            return config;
        }

        public static IApplicationBuilder UseMyOrleansDashboard(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                var authentication = await context.AuthenticateAsync();

                if (authentication.Succeeded && authentication.Principal.IsInRole(SquidexRoles.Administrator))
                {
                    await next();
                }
                else
                {
                    await context.ChallengeAsync(new AuthenticationProperties
                    {
                        RedirectUri = context.Request.PathBase + context.Request.Path
                    });
                }
            });

            app.UseOrleansDashboard();

            return app;
        }
    }
}
