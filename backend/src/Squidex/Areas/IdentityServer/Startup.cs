﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Squidex.Areas.IdentityServer.Config;
using Squidex.Web;

namespace Squidex.Areas.IdentityServer
{
    public static class Startup
    {
        public static void ConfigureIdentityServer(this IApplicationBuilder app)
        {
            var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

            app.Map(Constants.IdentityServerPrefix, identityApp =>
            {
                if (!environment.IsDevelopment())
                {
                    identityApp.UseDeveloperExceptionPage();
                }
                else
                {
                    identityApp.UseExceptionHandler("/error");
                }

                identityApp.UseRouting();

                identityApp.UseAuthentication();
                identityApp.UseAuthorization();

                identityApp.UseSquidexIdentityServer();

                identityApp.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            });
        }
    }
}
