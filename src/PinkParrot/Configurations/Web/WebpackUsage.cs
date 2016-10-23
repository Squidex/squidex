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

namespace PinkParrot.Configurations.Web
{
    public static class WebpackExtensions
    {

        public static IApplicationBuilder UseWebpackBuilder(this IApplicationBuilder app)
        {
            app.ApplicationServices.GetRequiredService<WebpackRunner>().Execute();

            return app;
        }

        public static IApplicationBuilder UseWebpackProxy(this IApplicationBuilder app)
        {
            app.UseMiddleware<WebpackMiddleware>();

            return app;
        }
    }
}
