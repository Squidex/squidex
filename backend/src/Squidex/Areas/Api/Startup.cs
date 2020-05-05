// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Builder;
using Squidex.Areas.Api.Config;
using Squidex.Areas.Api.Config.OpenApi;
using Squidex.Web;

namespace Squidex.Areas.Api
{
    public static class Startup
    {
        public static void ConfigureApi(this IApplicationBuilder app)
        {
            app.Map(Constants.ApiPrefix, appApi =>
            {
                appApi.UseMiddleware<IdentityServerPathMiddleware>();

                appApi.UseRouting();

                appApi.UseAuthentication();
                appApi.UseAuthorization();

                appApi.UseSquidexOpenApi();

                appApi.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            });
        }
    }
}
