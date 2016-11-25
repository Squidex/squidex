// ==========================================================================
//  WebpackUsage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Pipeline;

namespace Squidex.Config.Web
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
