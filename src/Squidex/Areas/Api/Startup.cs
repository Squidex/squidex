﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Builder;
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
                appApi.UseMyOpenApi();
                appApi.UseMvc();
            });
        }
    }
}
