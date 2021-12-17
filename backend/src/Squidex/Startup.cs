// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Areas.Api;
using Squidex.Areas.Api.Config.OpenApi;
using Squidex.Areas.Frontend;
using Squidex.Areas.IdentityServer;
using Squidex.Areas.IdentityServer.Config;
using Squidex.Areas.OrleansDashboard;
using Squidex.Areas.Portal;
using Squidex.Config.Authentication;
using Squidex.Config.Domain;
using Squidex.Config.Orleans;
using Squidex.Config.Startup;
using Squidex.Config.Web;
using Squidex.Pipeline.Plugins;
using Squidex.Web.Pipeline;

namespace Squidex
{
    public sealed class Startup
    {
        private readonly bool isResizer;
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment environment;

        public Startup(IConfiguration config, IWebHostEnvironment environment)
        {
            this.config = config;

            this.environment = environment;

            isResizer = config.GetValue<bool>("assets:resizer");
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddMemoryCache();
            services.AddHealthChecks();
            services.AddNonBreakingSameSiteCookies();
            services.AddDefaultWebServices(config);
            services.AddDefaultForwardRules();

            // Step 0: Log all configuration.
            services.AddHostedService<LogConfigurationHost>();

            // Step 1: Initialize all services.
            services.AddInitializer();

            services.AddSquidexImageResizing(config);
            services.AddSquidexAssetInfrastructure(config);
            services.AddSquidexSerializers();

            if (isResizer)
            {
                return;
            }

            services.AddSquidexMvcWithPlugins(config);

            services.AddSquidexApps(config);
            services.AddSquidexAssets(config);
            services.AddSquidexAuthentication(config);
            services.AddSquidexBackups();
            services.AddSquidexCommands(config);
            services.AddSquidexComments();
            services.AddSquidexContents(config);
            services.AddSquidexControllerServices(config);
            services.AddSquidexEventPublisher(config);
            services.AddSquidexEventSourcing(config);
            services.AddSquidexGraphQL();
            services.AddSquidexHealthChecks(config);
            services.AddSquidexHistory(config);
            services.AddSquidexIdentity(config);
            services.AddSquidexIdentityServer();
            services.AddSquidexInfrastructure(config);
            services.AddSquidexLocalization();
            services.AddSquidexMigration(config);
            services.AddSquidexNotifications(config);
            services.AddSquidexOpenApiSettings();
            services.AddSquidexQueries(config);
            services.AddSquidexRules(config);
            services.AddSquidexSchemas();
            services.AddSquidexSearch();
            services.AddSquidexStoreServices(config);
            services.AddSquidexSubscriptions(config);
            services.AddSquidexTelemetry(config);
            services.AddSquidexTranslation(config);
            services.AddSquidexUsageTracking(config);

            // Step 3: Start Orleans.
            services.AddOrleans(config, environment, builder => builder.ConfigureForSquidex(config));

            // Step 4: Run migration.
            services.AddHostedService<MigratorHost>();

            // Step 5: Run rebuild processes.
            services.AddHostedService<MigrationRebuilderHost>();

            // Step 6: Start background processes.
            services.AddBackgroundProcesses();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCookiePolicy();

            app.UseDefaultPathBase();
            app.UseDefaultForwardRules();

            app.UseSquidexImageResizing();
            app.UseSquidexHealthCheck();

            if (isResizer)
            {
                return;
            }

            app.UseSquidexRobotsTxt();
            app.UseSquidexCacheKeys();
            app.UseSquidexExceptionHandling();
            app.UseSquidexUsage();
            app.UseSquidexLogging();
            app.UseSquidexLocalization();
            app.UseSquidexLocalCache();
            app.UseSquidexCors();

            app.ConfigureDev();
            app.ConfigureApi();
            app.ConfigurePortal();
            app.ConfigureOrleansDashboard();
            app.ConfigureIdentityServer();
            app.ConfigureFrontend();

            app.UsePlugins();
        }
    }
}
