// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Squidex.Areas.Api.Controllers.News.Service;
using Squidex.Domain.Apps.Entities.Apps.Diagnostics;
using Squidex.Domain.Apps.Entities.Rules.UsageTracking;
using Squidex.Domain.Users;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Diagnostics;
using Squidex.Infrastructure.EventSourcing.Grains;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.UsageTracking;
using Squidex.Shared.Users;

#pragma warning disable RECS0092 // Convert field to readonly

namespace Squidex.Config.Domain
{
    public static class InfrastructureServices
    {
        public static void AddMyInfrastructureServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddHealthChecks()
                .AddCheck<GCHealthCheck>("GC", tags: new[] { "node" })
                .AddCheck<OrleansHealthCheck>("Orleans", tags: new[] { "cluster" })
                .AddCheck<OrleansAppsHealthCheck>("Orleans App", tags: new[] { "cluster" });

            services.AddSingletonAs(c => new CachingUsageTracker(
                    c.GetRequiredService<BackgroundUsageTracker>(),
                    c.GetRequiredService<IMemoryCache>()))
                .As<IUsageTracker>();

            services.AddSingletonAs(_ => SystemClock.Instance)
                .As<IClock>();

            services.AddSingletonAs<FeaturesService>()
                .AsSelf();

            services.AddSingletonAs<BackgroundUsageTracker>()
                .AsSelf();

            services.AddSingletonAs<GrainBootstrap<IEventConsumerManagerGrain>>()
                .AsSelf();

            services.AddSingletonAs<GrainBootstrap<IUsageTrackerGrain>>()
                .AsSelf();

            services.AddSingletonAs<LanguagesInitializer>()
                .AsSelf();

            services.AddSingletonAs<DeepLTranslator>()
                .As<ITranslator>();

            services.AddSingletonAs<AsyncLocalCache>()
                .As<ILocalCache>();

            services.AddSingletonAs<HttpContextAccessor>()
                .As<IHttpContextAccessor>();

            services.AddSingletonAs<ActionContextAccessor>()
                .As<IActionContextAccessor>();

            services.AddSingletonAs<DefaultUserResolver>()
                .AsOptional<IUserResolver>();

            services.AddSingletonAs<AssetUserPictureStore>()
                .AsOptional<IUserPictureStore>();

            services.AddSingletonAs<DefaultXmlRepository>()
                .As<IXmlRepository>();
        }
    }
}
