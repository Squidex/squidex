// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Areas.Api.Config.OpenApi;
using Squidex.Web;
using Squidex.Web.Pipeline;

namespace Squidex.Areas.Api
{
    public static class Startup
    {
        public static void ConfigureApi(this IApplicationBuilder app)
        {
            app.Map(Constants.PrefixApi, builder =>
            {
                builder.UseAccessTokenQueryString();

                builder.UseRouting();

                builder.UseAuthentication();
                builder.UseAuthorization();

                builder.UseSquidexOpenApi();

                builder.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            });
        }
    }
}
