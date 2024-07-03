// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using NodaTime;
using Squidex.AI;
using Squidex.Areas.Api.Controllers.Contents.Generator;
using Squidex.Areas.Api.Controllers.News;
using Squidex.Areas.Api.Controllers.News.Service;
using Squidex.Areas.Api.Controllers.UI;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.Scripting.Extensions;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Core.Templates;
using Squidex.Domain.Apps.Core.Templates.Extensions;
using Squidex.Domain.Apps.Entities.Contents.Counter;
using Squidex.Domain.Apps.Entities.Tags;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Diagnostics;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.UsageTracking;
using Squidex.Pipeline.Robots;
using Squidex.Shared;
using Squidex.Web;
using Squidex.Web.Pipeline;

namespace Squidex.Config.Domain;

public static class InfrastructureServices
{
    public static void AddSquidexInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<ExposedConfiguration>(config,
            "exposedConfiguration");

        services.Configure<JintScriptOptions>(config,
            "scripting");

        services.Configure<DiagnoserOptions>(config,
            "diagnostics");

        services.AddHttpClient("Jint");

        services.AddReplicatedCache();
        services.AddAsyncLocalCache();
        services.AddBackgroundCache();

        services.AddSingletonAs(_ => SystemClock.Instance)
            .As<IClock>();

        services.AddSingletonAs<BackgroundRequestLogStore>()
            .AsOptional<IRequestLogStore>();

        services.AddSingletonAs<Diagnoser>()
            .AsSelf();

        services.AddSingletonAs<ScriptingCompleter>()
            .AsSelf();

        services.AddSingletonAs<JintScriptEngine>()
            .As<IScriptEngine>().As<IScriptDescriptor>();

        services.AddSingletonAs<TagService>()
            .As<ITagService>();

        services.AddSingletonAs<CounterJintExtension>()
            .As<IJintExtension>().As<IScriptDescriptor>();

        services.AddSingletonAs<DateTimeJintExtension>()
            .As<IJintExtension>().As<IScriptDescriptor>();

        services.AddSingletonAs<StringJintExtension>()
            .As<IJintExtension>().As<IScriptDescriptor>();

        services.AddSingletonAs<StringWordsJintExtension>()
            .As<IJintExtension>().As<IScriptDescriptor>();

        services.AddSingletonAs<StringAsyncJintExtension>()
            .As<IJintExtension>().As<IScriptDescriptor>();

        services.AddSingletonAs<HttpJintExtension>()
            .As<IJintExtension>().As<IScriptDescriptor>();

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
    }

    public static void AddSquidexUsageTracking(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<UsageOptions>(config,
            "usage");

        services.AddSingletonAs(c => new CachingUsageTracker(
                c.GetRequiredService<BackgroundUsageTracker>(),
                c.GetRequiredService<IMemoryCache>()))
            .As<IUsageTracker>();

        services.AddSingletonAs<ApiUsageTracker>()
            .As<IApiUsageTracker>();

        services.AddSingletonAs<BackgroundUsageTracker>()
            .AsSelf();
    }

    public static void AddSquidexTranslation(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<LanguagesOptions>(config,
            "languages");

        services.Configure<ChatOptions>(config,
            "chatbot");

        services.AddSingletonAs<LanguagesInitializer>()
            .AsSelf();

        services.AddAI();

        var apiKey = config["chatBot:openAi:apiKey"];

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            services.AddOpenAIChat(config);
            services.AddAIImagePipe();
            services.AddDallE(config, options =>
            {
                options.DownloadImage = true;

                if (string.IsNullOrEmpty(options.ApiKey))
                {
                    options.ApiKey = apiKey;
                }
            });
        }

        services.AddDeepLTranslations(config);
        services.AddGoogleCloudTranslations(config);
    }

    public static void AddSquidexLocalization(this IServiceCollection services)
    {
        var translator = new ResourcesLocalizer(Texts.ResourceManager);

        T.Setup(translator);

        services.AddSingletonAs(c => translator)
            .As<ILocalizer>();
    }

    public static void AddSquidexControllerServices(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<RobotsTxtOptions>(config,
            "robots");

        services.Configure<CachingOptions>(config,
            "caching");

        services.Configure<MyUIOptions>(config,
            "ui");

        services.Configure<MyNewsOptions>(config,
            "news");

        services.AddSingletonAs<FeaturesService>()
            .AsSelf();

        services.AddSingletonAs<SchemasOpenApiGenerator>()
            .AsSelf();
    }
}
