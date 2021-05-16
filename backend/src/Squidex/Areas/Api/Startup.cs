// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Builder;
using Squidex.Areas.Api.Config.OpenApi;
using Squidex.Web;
using Squidex.Web.Pipeline;

namespace Squidex.Areas.Api
{
    public static class Startup
    {
        public static void ConfigureApi(this IApplicationBuilder app)
        {
            app.Map(Constants.PrefixApi, appApi =>
            {
                appApi.UseAccessTokenQueryString();

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
