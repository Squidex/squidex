// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Web;

namespace Squidex.Areas.IdentityServer
{
    public static class Startup
    {
        public static void ConfigureIdentityServer(this IApplicationBuilder app)
        {
            var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

            app.Map(Constants.PrefixIdentityServer, identityApp =>
            {
                if (environment.IsDevelopment())
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

                identityApp.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            });
        }
    }
}
