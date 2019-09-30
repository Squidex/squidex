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
using Migrate_01;
using Squidex.Areas.Api;
using Squidex.Areas.Api.Config.OpenApi;
using Squidex.Areas.Api.Controllers.Contents;
using Squidex.Areas.Api.Controllers.News;
using Squidex.Areas.Api.Controllers.UI;
using Squidex.Areas.Frontend;
using Squidex.Areas.IdentityServer;
using Squidex.Areas.IdentityServer.Config;
using Squidex.Areas.OrleansDashboard;
using Squidex.Areas.Portal;
using Squidex.Config;
using Squidex.Config.Authentication;
using Squidex.Config.Domain;
using Squidex.Config.Startup;
using Squidex.Config.Web;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Diagnostics;
using Squidex.Infrastructure.Translations;
using Squidex.Pipeline.Plugins;
using Squidex.Pipeline.Robots;
using Squidex.Web;
using Squidex.Web.Pipeline;

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
            services.AddHttpClient();
            services.AddLogging();
            services.AddMemoryCache();
            services.AddOptions();

            services.AddMyMvcWithPlugins(config);

            services.AddMyAssetServices(config);
            services.AddMyAuthentication(config);
            services.AddMyEntitiesServices(config);
            services.AddMyEventPublishersServices(config);
            services.AddMyEventStoreServices(config);
            services.AddMyIdentityServer();
            services.AddMyInfrastructureServices(config);
            services.AddMyLoggingServices(config);
            services.AddMyOpenApiSettings();
            services.AddMyMigrationServices();
            services.AddMyRuleServices();
            services.AddMySerializers();
            services.AddMyStoreServices(config);
            services.AddMySubscriptionServices(config);

            services.Configure<ContentOptions>(
                config.GetSection("contents"));
            services.Configure<AssetOptions>(
                config.GetSection("assets"));
            services.Configure<DeepLTranslatorOptions>(
                config.GetSection("translations:deepL"));
            services.Configure<LanguagesOptions>(
                config.GetSection("languages"));
            services.Configure<ReadonlyOptions>(
                config.GetSection("mode"));
            services.Configure<RobotsTxtOptions>(
                config.GetSection("robots"));
            services.Configure<GCHealthCheckOptions>(
                config.GetSection("healthz:gc"));
            services.Configure<ETagOptions>(
                config.GetSection("etags"));
            services.Configure<UrlsOptions>(
                config.GetSection("urls"));
            services.Configure<UsageOptions>(
                config.GetSection("usage"));
            services.Configure<RebuildOptions>(
                config.GetSection("rebuild"));
            services.Configure<ExposedConfiguration>(
                config.GetSection("exposedConfiguration"));
            services.Configure<RuleOptions>(
                config.GetSection("rules"));

            services.Configure<MyContentsControllerOptions>(
                config.GetSection("contentsController"));
            services.Configure<MyIdentityOptions>(
                config.GetSection("identity"));
            services.Configure<MyUIOptions>(
                config.GetSection("ui"));
            services.Configure<MyNewsOptions>(
                config.GetSection("news"));
        }

        public void Configure(IApplicationBuilder app)
        {
            app.ApplicationServices.LogConfiguration();

            app.UsePluginsBefore();

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

            app.UsePluginsAfter();
            app.UsePlugins();
        }
    }
}
