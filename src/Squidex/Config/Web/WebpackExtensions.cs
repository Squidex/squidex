// ==========================================================================
//  WebpackExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Builder;
using Squidex.Pipeline;

namespace Squidex.Config.Web
{
    public static class WebpackExtensions
    {
        public static IApplicationBuilder UseWebpackProxy(this IApplicationBuilder app)
        {
            app.UseMiddleware<WebpackMiddleware>();

            return app;
        }

        public static IApplicationBuilder UseMyTracking(this IApplicationBuilder app)
        {
            app.UseMiddleware<LogPerformanceMiddleware>();
            app.UseMiddleware<AppTrackingMiddleware>();

            return app;
        }
    }
}
