﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans;
using Squidex.Areas.Api.Config.OpenApi;
using Squidex.Areas.Frontend;
using Squidex.Areas.IdentityServer.Config;
using Squidex.Areas.OrleansDashboard.Middlewares;
using Squidex.Areas.Portal.Middlewares;
using Squidex.Config.Authentication;
using Squidex.Config.Domain;
using Squidex.Config.Web;
using Squidex.Pipeline.Plugins;
using Squidex.Web;
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
            services.AddHealthChecks();
            services.AddDefaultWebServices(config);
            services.AddDefaultForwardRules();

            // They must be called in this order.
            services.AddSquidexMvcWithPlugins(config);
            services.AddSquidexIdentity(config);
            services.AddSquidexIdentityServer();
            services.AddSquidexAuthentication(config);

            services.AddSquidexImageResizing(config);
            services.AddSquidexAssetInfrastructure(config);
            services.AddSquidexSerializers();
            services.AddSquidexApps(config);
            services.AddSquidexAssets(config);
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
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCookiePolicy();

            app.UseDefaultPathBase();
            app.UseDefaultForwardRules();

            app.UseSquidexHealthCheck();
            app.UseSquidexRobotsTxt();
            app.UseSquidexLogging();
            app.UseSquidexLocalization();
            app.UseSquidexLocalCache();
            app.UseSquidexCors();
            app.UseOpenApi(options =>
            {
                options.Path = "/api/swagger/v1/swagger.json";
            });

            app.UseWhen(c => c.Request.Path.StartsWithSegments(Constants.PrefixIdentityServer, StringComparison.OrdinalIgnoreCase), builder =>
            {
                builder.UseExceptionHandler("/identity-server/error");
            });

            app.UseWhen(c => c.Request.Path.StartsWithSegments(Constants.PrefixApi, StringComparison.OrdinalIgnoreCase), builder =>
            {
                builder.UseSquidexCacheKeys();
                builder.UseSquidexExceptionHandling();
                builder.UseSquidexUsage();
                builder.UseAccessTokenQueryString();
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.Map(Constants.PrefixPortal, builder =>
            {
                builder.UseMiddleware<PortalDashboardAuthenticationMiddleware>();
                builder.UseMiddleware<PortalRedirectMiddleware>();
            });

            app.Map(Constants.PrefixOrleans, builder =>
            {
                builder.UseMiddleware<OrleansDashboardAuthenticationMiddleware>();
                builder.UseOrleansDashboard();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Return a 404 for all unresolved api requests.
            app.Map(Constants.PrefixApi, builder =>
            {
                builder.Use404();
            });

            app.UseFrontend();

            app.UsePlugins();
        }
    }
}
