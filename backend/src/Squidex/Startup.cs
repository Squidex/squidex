// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Areas.Api;
using Squidex.Areas.Api.Config.OpenApi;
using Squidex.Areas.Frontend;
using Squidex.Areas.IdentityServer;
using Squidex.Areas.IdentityServer.Config;
using Squidex.Areas.OrleansDashboard;
using Squidex.Areas.Portal;
using Squidex.Config.Authentication;
using Squidex.Config.Domain;
using Squidex.Config.Web;
using Squidex.Pipeline.Plugins;
using Squidex.Web.Pipeline;

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
            services.AddHttpClient();
            services.AddMemoryCache();
            services.AddNonBreakingSameSiteCookies();

            services.AddSquidexMvcWithPlugins(config);

            services.AddSquidexApps(config);
            services.AddSquidexAssetInfrastructure(config);
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
            services.AddSquidexSerializers();
            services.AddSquidexStoreServices(config);
            services.AddSquidexSubscriptions(config);
            services.AddSquidexTelemetry(config);
            services.AddSquidexTranslation(config);
            services.AddSquidexUsageTracking(config);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCookiePolicy();

            app.UseDefaultPathBase();
            app.UseDefaultForwardRules();

            app.UseSquidexCacheKeys();
            app.UseSquidexHealthCheck();
            app.UseSquidexRobotsTxt();
            app.UseSquidexTracking();
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
