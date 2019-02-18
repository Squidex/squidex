// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Areas.Api;
using Squidex.Areas.Api.Config.Swagger;
using Squidex.Areas.Api.Controllers.Contents;
using Squidex.Areas.Api.Controllers.News;
using Squidex.Areas.Frontend;
using Squidex.Areas.IdentityServer;
using Squidex.Areas.IdentityServer.Config;
using Squidex.Areas.OrleansDashboard;
using Squidex.Areas.Portal;
using Squidex.Config;
using Squidex.Config.Authentication;
using Squidex.Config.Domain;
using Squidex.Config.Orleans;
using Squidex.Config.Startup;
using Squidex.Config.Web;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Extensions.Actions.Twitter;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Diagnostics;
using Squidex.Pipeline;
using Squidex.Pipeline.Robots;

namespace Squidex
{
    public sealed class WebStartup
    {
        private readonly IConfiguration configuration;

        public WebStartup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var config = configuration;

            services.AddHttpClient();
            services.AddLogging();
            services.AddMemoryCache();
            services.AddOptions();

            services.AddMyAssetServices(config);
            services.AddMyAuthentication(config);
            services.AddMyEntitiesServices(config);
            services.AddMyEventPublishersServices(config);
            services.AddMyEventStoreServices(config);
            services.AddMyIdentityServer();
            services.AddMyInfrastructureServices(config);
            services.AddMyLoggingServices(config);
            services.AddMyMigrationServices();
            services.AddMyMvc();
            services.AddMyRuleServices();
            services.AddMySerializers();
            services.AddMyStoreServices(config);
            services.AddMySwaggerSettings();
            services.AddMySubscriptionServices(config);

            services.Configure<ContentOptions>(
                config.GetSection("contents"));
            services.Configure<AssetOptions>(
                config.GetSection("assets"));
            services.Configure<ReadonlyOptions>(
                config.GetSection("mode"));
            services.Configure<TwitterOptions>(
                config.GetSection("twitter"));
            services.Configure<RobotsTxtOptions>(
                config.GetSection("robots"));
            services.Configure<GCHealthCheckOptions>(
                config.GetSection("healthz:gc"));
            services.Configure<ETagOptions>(
                config.GetSection("etags"));

            services.Configure<MyContentsControllerOptions>(
                config.GetSection("contentsController"));
            services.Configure<MyUrlsOptions>(
                config.GetSection("urls"));
            services.Configure<MyIdentityOptions>(
                config.GetSection("identity"));
            services.Configure<MyUIOptions>(
                config.GetSection("ui"));
            services.Configure<MyUsageOptions>(
                config.GetSection("usage"));
            services.Configure<MyNewsOptions>(
                config.GetSection("news"));

            var provider = services.AddAndBuildOrleans(configuration, afterServices =>
            {
                afterServices.AddHostedService<InitializerHost>();
                afterServices.AddHostedService<MigratorHost>();
            });

            return provider;
        }

        public void Configure(IApplicationBuilder app)
        {
            app.ApplicationServices.LogConfiguration();

            app.UseMyHealthCheck();
            app.UseMyRobotsTxt();
            app.UseMyTracking();
            app.UseMyLocalCache();
            app.UseMyCors();
            app.UseMyForwardingRules();

            app.ConfigureApi();
            app.ConfigurePortal();
            app.ConfigureOrleansDashboard();
            app.ConfigureIdentityServer();
            app.ConfigureFrontend();
        }
    }
}
