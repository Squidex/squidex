// ==========================================================================
//  Services.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Areas.Api.Config.Swagger;
using Squidex.Areas.IdentityServer.Config;
using Squidex.Config;
using Squidex.Config.Authentication;
using Squidex.Config.Domain;
using Squidex.Config.Web;
using Squidex.Domain.Apps.Core.Apps;

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
            services.AddMyEventPublishersServices(config);
            services.AddMyEventStoreServices(config);
            services.AddMyIdentityServer();
            services.AddMyInfrastructureServices(config);
            services.AddMyMvc();
            services.AddMyPubSubServices(config);
            services.AddMyReadServices(config);
            services.AddMySerializers();
            services.AddMyStoreServices(config);
            services.AddMySwaggerSettings();
            services.AddMyWriteServices();

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
