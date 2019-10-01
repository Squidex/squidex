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
using Squidex.Config.Web;
using Squidex.Pipeline.Plugins;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Squidex
{
    public sealed class Startup
    {
        private readonly IConfiguration config;

        public Startup(IConfiguration config)
        {
            this.config = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSquidexMvcWithPlugins(config);

            services.AddSquidexApps();
            services.AddSquidexAssetInfrastructure(config);
            services.AddSquidexAssets(config);
            services.AddSquidexBackups();
            services.AddSquidexCommands(config);
            services.AddSquidexComments();
            services.AddSquidexContents(config);
            services.AddSquidexEventPublisher(config);
            services.AddSquidexEventSourcing(config);
            services.AddSquidexHealthChecks(config);
            services.AddSquidexHistory();
            services.AddSquidexInfrastructure(config);
            services.AddSquidexMigration(config);
            services.AddSquidexNotifications(config);
            services.AddSquidexQueries(config);
            services.AddSquidexRules(config);
            services.AddSquidexSchemas();
            services.AddSquidexSerializers();
            services.AddSquidexStoreServices(config);
            services.AddSquidexSubscriptions(config);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UsePluginsBefore();

            app.UseSquidexHealthCheck();
            app.UseSquidexRobotsTxt();
            app.UseSquidexTracking();
            app.UseSquidexLocalCache();
            app.UseSquidexCors();
            app.UseSquidexForwardingRules();

            app.ConfigureApi();
            app.ConfigurePortal();
            app.ConfigureOrleansDashboard();
            app.ConfigureIdentityServer();
            app.ConfigureFrontend();

            app.UsePluginsAfter();
            app.UsePlugins();
        }
    }
}
