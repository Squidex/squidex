// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Builder;
using Orleans;
using Squidex.Areas.OrleansDashboard.Middlewares;
using Squidex.Web;

namespace Squidex.Areas.OrleansDashboard
{
    public static class Startup
    {
        public static void ConfigureOrleansDashboard(this IApplicationBuilder app)
        {
            app.Map(Constants.PrefixOrleans, orleansApp =>
            {
                orleansApp.UseAuthentication();
                orleansApp.UseAuthorization();

                orleansApp.UseMiddleware<OrleansDashboardAuthenticationMiddleware>();
                orleansApp.UseOrleansDashboard();
            });
        }
    }
}
