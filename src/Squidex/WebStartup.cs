// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Areas.Api;
using Squidex.Areas.Frontend;
using Squidex.Areas.IdentityServer;
using Squidex.Areas.Portal;
using Squidex.Config.Domain;
using Squidex.Config.Web;

namespace Squidex
{
    public sealed class WebStartup : IStartup
    {
        private readonly IConfiguration configuration;

        public WebStartup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddAppServices(configuration);

            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.ApplicationServices.LogConfiguration();
            app.ApplicationServices.InitializeAll();
            app.ApplicationServices.Migrate();
            app.ApplicationServices.RunAll();

            app.UseMyCors();
            app.UseMyForwardingRules();
            app.UseMyTracking();

            app.ConfigureApi();
            app.ConfigurePortal();
            app.ConfigureIdentityServer();

            app.ConfigureFrontend();
        }
    }
}
