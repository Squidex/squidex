// ==========================================================================
//  Startup.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Builder;
using Orleans;
using Squidex.Areas.OrleansDashboard.Middlewares;

namespace Squidex.Areas.OrleansDashboard
{
    public static class Startup
    {
        public static void ConfigureOrleansDashboard(this IApplicationBuilder app)
        {
            app.Map("/orleans", orleansApp =>
            {
                orleansApp.UseMiddleware<OrleansDashboardAuthenticationMiddleware>();
                orleansApp.UseOrleansDashboard();
            });
        }
    }
}
