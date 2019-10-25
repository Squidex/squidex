﻿// ==========================================================================
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
using Squidex.Areas.Api.Controllers.Contents;
using Squidex.Areas.Api.Controllers.Contents.Generator;
using Squidex.Areas.Api.Controllers.News;
using Squidex.Areas.Api.Controllers.News.Service;
using Squidex.Areas.Api.Controllers.UI;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Entities.Rules.UsageTracking;
using Squidex.Domain.Apps.Entities.Tags;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing.Grains;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.UsageTracking;
using Squidex.Pipeline.Robots;
using Squidex.Web;
using Squidex.Web.Pipeline;

#pragma warning disable RECS0092 // Convert field to readonly

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

            services.AddSingletonAs(_ => SystemClock.Instance)
                .As<IClock>();

            services.AddSingletonAs<GrainBootstrap<IEventConsumerManagerGrain>>()
                .AsSelf();

            services.AddSingletonAs<GrainTagService>()
                .As<ITagService>();

            services.AddSingletonAs<AsyncLocalCache>()
                .As<ILocalCache>();

            services.AddSingletonAs<JintScriptEngine>()
                .AsOptional<IScriptEngine>();

            services.AddSingleton<Func<IGrainCallContext, string>>(DomainObjectGrainFormatter.Format);
        }

        public static void AddSquidexUsageTracking(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<UsageOptions>(
                config.GetSection("usage"));

            services.AddSingletonAs(c => new CachingUsageTracker(
                    c.GetRequiredService<BackgroundUsageTracker>(),
                    c.GetRequiredService<IMemoryCache>()))
                .As<IUsageTracker>();

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
            services.Configure<ETagOptions>(
                config.GetSection("etags"));
            services.Configure<MyContentsControllerOptions>(
                config.GetSection("contentsController"));
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
