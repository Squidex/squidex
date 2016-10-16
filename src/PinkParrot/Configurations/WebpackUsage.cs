// ==========================================================================
//  WebpackUsage.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using PinkParrot.Pipeline;

namespace PinkParrot.Configurations
{
    public static class WebpackExtensions
    {
        public static IServiceCollection AddWebpack(this IServiceCollection services)
        {
            services.AddSingleton<WebpackRunner>();

            return services;
        }

        public static IApplicationBuilder UseWebpack(this IApplicationBuilder app)
        {
            app.ApplicationServices.GetService<WebpackRunner>().Execute();

            app.UseMiddleware<WebpackMiddleware>();

            return app;
        }
    }
}
