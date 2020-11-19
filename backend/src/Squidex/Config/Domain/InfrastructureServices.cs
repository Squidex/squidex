// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Orleans;
using Squidex.Areas.Api.Controllers.Contents.Generator;
using Squidex.Areas.Api.Controllers.News;
using Squidex.Areas.Api.Controllers.News.Service;
using Squidex.Areas.Api.Controllers.UI;
using Squidex.Caching;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.Scripting.Extensions;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Core.Templates;
using Squidex.Domain.Apps.Core.Templates.Extensions;
using Squidex.Domain.Apps.Entities.Contents.Counter;
using Squidex.Domain.Apps.Entities.Rules.UsageTracking;
using Squidex.Domain.Apps.Entities.Tags;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing.Grains;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.UsageTracking;
using Squidex.Pipeline.Robots;
using Squidex.Web;
using Squidex.Web.Pipeline;

namespace Squidex.Config.Domain
{
    public static class InfrastructureServices
    {
        public static void AddSquidexInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<UrlsOptions>(
                config.GetSection("urls"));
            services.Configure<ExposedConfiguration>(
                config.GetSection("exposedConfiguration"));
            services.Configure<ReplicatedCacheOptions>(
                config.GetSection("caching:replicated"));

            services.AddReplicatedCache();
            services.AddAsyncLocalCache();

            services.AddSingletonAs(_ => SystemClock.Instance)
                .As<IClock>();

            services.AddSingletonAs<GrainBootstrap<IEventConsumerManagerGrain>>()
                .AsSelf();

            services.AddSingletonAs<GrainTagService>()
                .As<ITagService>();

            services.AddSingletonAs<JintScriptEngine>()
                .AsOptional<IScriptEngine>();

            services.AddSingletonAs<CounterJintExtension>()
                .As<IJintExtension>();

            services.AddSingletonAs<DateTimeJintExtension>()
                .As<IJintExtension>();

            services.AddSingletonAs<StringJintExtension>()
                .As<IJintExtension>();

            services.AddSingletonAs<StringWordsJintExtension>()
                .As<IJintExtension>();

            services.AddSingletonAs<HttpJintExtension>()
                .As<IJintExtension>();

            services.AddSingletonAs<FluidTemplateEngine>()
                .AsOptional<ITemplateEngine>();

            services.AddSingletonAs<ContentFluidExtension>()
                .As<IFluidExtension>();

            services.AddSingletonAs<DateTimeFluidExtension>()
                .As<IFluidExtension>();

            services.AddSingletonAs<StringFluidExtension>()
                .As<IFluidExtension>();

            services.AddSingletonAs<StringWordsFluidExtension>()
                .As<IFluidExtension>();

            services.AddSingletonAs<UserFluidExtension>()
                .As<IFluidExtension>();

            services.AddSingleton<Func<IIncomingGrainCallContext, string>>(DomainObjectGrainFormatter.Format);
        }

        public static void AddSquidexUsageTracking(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<UsageOptions>(
                config.GetSection("usage"));

            services.AddSingletonAs(c => new CachingUsageTracker(
                    c.GetRequiredService<BackgroundUsageTracker>(),
                    c.GetRequiredService<IMemoryCache>()))
                .As<IUsageTracker>();

            services.AddSingletonAs<ApiUsageTracker>()
                .As<IApiUsageTracker>();

            services.AddSingletonAs<BackgroundUsageTracker>()
                .AsSelf();

            services.AddSingletonAs<GrainBootstrap<IUsageTrackerGrain>>()
                .AsSelf();
        }

        public static void AddSquidexTranslation(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<DeepLTranslatorOptions>(
                config.GetSection("translations:deepL"));
            services.Configure<LanguagesOptions>(
                config.GetSection("languages"));

            services.AddSingletonAs<LanguagesInitializer>()
                .AsSelf();

            services.AddSingletonAs<DeepLTranslator>()
                .As<ITranslator>();
        }

        public static void AddSquidexControllerServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<RobotsTxtOptions>(
                config.GetSection("robots"));
            services.Configure<CachingOptions>(
                config.GetSection("caching"));
            services.Configure<MyUIOptions>(
                config.GetSection("ui"));
            services.Configure<MyNewsOptions>(
                config.GetSection("news"));

            services.AddSingletonAs<FeaturesService>()
                .AsSelf();

            services.AddSingletonAs<SchemasOpenApiGenerator>()
                .AsSelf();
        }
    }
}
