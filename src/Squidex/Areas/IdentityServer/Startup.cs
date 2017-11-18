// ==========================================================================
//  Startup.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Areas.IdentityServer.Config;
using Squidex.Config;

namespace Squidex.Areas.IdentityServer
{
    public static class Startup
    {
        public static void ConfigureIdentityServer(this IApplicationBuilder app)
        {
            app.ApplicationServices.UseMyAdminRole();
            app.ApplicationServices.UseMyAdmin();

            var environment = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();

            app.Map(Constants.IdentityPrefix, identityApp =>
            {
                app.UseMyIdentityServer();

                if (environment.IsDevelopment())
                {
                    identityApp.UseDeveloperExceptionPage();
                }
                else
                {
                    identityApp.UseExceptionHandler("/error");
                }

                identityApp.UseStaticFiles();
                identityApp.UseMvc();
            });
        }
    }
}
