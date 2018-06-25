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
using Squidex.Infrastructure.Commands;

namespace Squidex
{
    public static class AppServices
    {
        public static void AddAppServices(this IServiceCollection services, IConfiguration config)
        {
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

            services.Configure<ReadonlyOptions>(
                config.GetSection("mode"));
            services.Configure<ContentsControllerOptions>(
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
