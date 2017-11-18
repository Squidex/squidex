// ==========================================================================
//  WebStartup.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Areas.Frontend;
using Squidex.Areas.OrleansDashboard;
using Squidex.Config;
using Squidex.Config.Domain;
using Squidex.Config.Identity;
using Squidex.Config.Orleans;
using Squidex.Config.Swagger;
using Squidex.Config.Web;

#pragma warning disable RECS0002 // Convert anonymous method to method group

namespace Squidex
{
    public class WebStartup : IStartup
    {
        private readonly IConfiguration configuration;
        private readonly IHostingEnvironment environment;
        private static readonly string[] IdentityServerPaths =
        {
            "/client-callback-popup",
            "/client-callback-silent",
            "/account",
            "/error"
        };

        public WebStartup(IConfiguration configuration, IHostingEnvironment environment)
        {
            this.configuration = configuration;
            this.environment = environment;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddAppClient();
            services.AddAppServices(configuration);

            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.ApplicationServices.LogConfiguration();
            app.ApplicationServices.TestExternalSystems();

            app.UseMyCors();
            app.UseMyForwardingRules();
            app.UseMyTracking();
            app.UseMyAuthentication();

            MapAndUseIdentityServer(app);
            MapAndUseApi(app);

            app.ConfigureOrleansDashboard();
            app.ConfigureFrontend();
        }

        private void MapAndUseIdentityServer(IApplicationBuilder app)
        {
            app.Map(Constants.IdentityPrefix, identityApp =>
            {
                if (environment.IsDevelopment())
                {
                    identityApp.UseDeveloperExceptionPage();
                }
                else
                {
                    identityApp.UseExceptionHandler("/error");
                }

                identityApp.UseMyIdentityServer();
                identityApp.UseMyAdminRole();
                identityApp.UseMyAdmin();
                identityApp.UseStaticFiles();

                identityApp.MapWhen(IsIdentityRequest, mvcApp =>
                {
                    mvcApp.UseMvc();
                });
            });
        }

        private void MapAndUseApi(IApplicationBuilder app)
        {
            app.Map(Constants.ApiPrefix, appApi =>
            {
                if (environment.IsDevelopment())
                {
                    appApi.UseDeveloperExceptionPage();
                }

                appApi.UseMySwagger();

                appApi.MapWhen(x => !IsIdentityRequest(x), mvcApp =>
                {
                    mvcApp.UseMvc();
                });
            });
        }

        private static bool IsIdentityRequest(HttpContext context)
        {
            return IdentityServerPaths.Any(p => context.Request.Path.StartsWithSegments(p));
        }
    }
}
