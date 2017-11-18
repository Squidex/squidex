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
using Squidex.Areas.Portal.Middlewares;

namespace Squidex.Areas.Portal
{
    public static class Startup
    {
        public static void ConfigurePortal(this IApplicationBuilder app)
        {
            app.Map("/portal", orleansApp =>
            {
                orleansApp.UseAuthentication();
                orleansApp.UseMiddleware<PortalDashboardAuthenticationMiddleware>();
                orleansApp.UseMiddleware<PortalRedirectMiddleware>();
            });
        }
    }
}
