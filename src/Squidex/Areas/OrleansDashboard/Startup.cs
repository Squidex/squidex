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
using Squidex.Config;

namespace Squidex.Areas.OrleansDashboard
{
    public static class Startup
    {
        public static void ConfigureOrleansDashboard(this IApplicationBuilder app)
        {
            app.Map(Constants.OrleansPrefix, orleansApp =>
            {
                orleansApp.UseAuthentication();
                orleansApp.UseMiddleware<OrleansDashboardAuthenticationMiddleware>();
                orleansApp.UseOrleansDashboard();
            });
        }
    }
}
