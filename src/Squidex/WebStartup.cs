// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Builder;
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
    public sealed class WebStartup
    {
        private readonly IConfiguration configuration;

        public WebStartup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOrleansSilo();
            services.AddAppServices(configuration);

            services.AddHostedService<SystemExtensions.InitializeHostedService>();
            services.AddHostedService<SystemExtensions.MigratorHostedService>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.ApplicationServices.LogConfiguration();

            app.UseMyLocalCache();
            app.UseMyCors();
            app.UseMyForwardingRules();
            app.UseMyTracking();

            app.ConfigureApi();
            app.ConfigurePortal();
            app.ConfigureOrleansDashboard();
            app.ConfigureIdentityServer();
            app.ConfigureFrontend();
        }
    }
}
