// ==========================================================================
//  WebStartup.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Areas.Api;
using Squidex.Areas.Frontend;
using Squidex.Areas.IdentityServer;
using Squidex.Areas.OrleansDashboard;
using Squidex.Areas.Portal;
using Squidex.Config.Domain;
using Squidex.Config.Orleans;
using Squidex.Config.Web;

namespace Squidex
{
    public class WebStartup : IStartup
    {
        private readonly IConfiguration configuration;

        public WebStartup(IConfiguration configuration)
        {
            this.configuration = configuration;
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

            app.ConfigureApi();
            app.ConfigureOrleansDashboard();
            app.ConfigurePortal();
            app.ConfigureIdentityServer();

            app.ConfigureFrontend();
        }
    }
}
