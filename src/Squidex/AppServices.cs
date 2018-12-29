// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Areas.Api.Config.Swagger;
using Squidex.Areas.Api.Controllers.Contents;
using Squidex.Areas.IdentityServer.Config;
using Squidex.Config;
using Squidex.Config.Authentication;
using Squidex.Config.Domain;
using Squidex.Config.Web;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Extensions.Actions.Email;
using Squidex.Extensions.Actions.Twitter;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Diagnostics;
using Squidex.Pipeline;
using Squidex.Pipeline.Robots;

namespace Squidex
{
    public static class AppServices
    {
        public static void AddAppServices(this IServiceCollection services, IConfiguration config)
        {
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
            services.AddMyInfrastructureServices();
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
            services.Configure<EmailOptions>(
                config.GetSection("smtpServer"));
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
        }
    }
}
