// ==========================================================================
//  InfrastructureServices.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NodaTime;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Actors;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Assets.ImageSharp;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.UsageTracking;
using Squidex.Pipeline;

namespace Squidex.Config.Domain
{
    public static class InfrastructureServices
    {
        public static void AddMyInfrastructureServices(this IServiceCollection services, IConfiguration config)
        {
            if (config.GetValue<bool>("logging:human"))
            {
                services.AddSingleton(c => new Func<IObjectWriter>(() => new JsonLogWriter(Formatting.Indented, true)));
            }
            else
            {
                services.AddSingleton(c => new Func<IObjectWriter>(() => new JsonLogWriter()));
            }

            var loggingFile = config.GetValue<string>("logging:file");

            if (!string.IsNullOrWhiteSpace(loggingFile))
            {
                services.AddSingleton(new FileChannel(loggingFile))
                    .As<ILogChannel>()
                    .As<IExternalSystem>();
            }

            services.AddSingleton(c => new ApplicationInfoLogAppender(typeof(Startup).Assembly, Guid.NewGuid()))
                .As<ILogAppender>();

            services.AddSingleton<ActionContextLogAppender>()
                .As<ILogAppender>();

            services.AddSingleton<TimestampLogAppender>()
                .As<ILogAppender>();

            services.AddSingleton<DebugLogChannel>()
                .As<ILogChannel>();

            services.AddSingleton<ConsoleLogChannel>()
                .As<ILogChannel>();

            services.AddSingleton<SemanticLog>()
                .As<ISemanticLog>();

            services.AddSingleton(SystemClock.Instance)
                .As<IClock>();

            services.AddSingleton<BackgroundUsageTracker>()
                .As<IUsageTracker>();

            services.AddSingleton<HttpContextAccessor>()
                .As<IHttpContextAccessor>();

            services.AddSingleton<ActionContextAccessor>()
                .As<IActionContextAccessor>();

            services.AddSingleton<DefaultDomainObjectRepository>()
                .As<IDomainObjectRepository>();

            services.AddSingleton<DefaultDomainObjectFactory>()
                .As<IDomainObjectFactory>();

            services.AddSingleton<AggregateHandler>()
                .As<IAggregateHandler>();

            services.AddSingleton<InMemoryCommandBus>()
                .As<ICommandBus>();

            services.AddSingleton<DefaultEventNotifier>()
                .As<IEventNotifier>();

            services.AddSingleton<DefaultStreamNameResolver>()
                .As<IStreamNameResolver>();

            services.AddSingleton<ImageSharpAssetThumbnailGenerator>()
                .As<IAssetThumbnailGenerator>();

            services.AddSingleton<DefaultRemoteActorChannel>()
                .As<IRemoteActorChannel>();

            services.AddSingleton<RemoteActors>()
                .As<IActors>();

            services.AddSingleton<EventConsumerCleaner>();
            services.AddSingleton<EventDataFormatter>();

            services.AddSingleton(c => new InvalidatingMemoryCache(
                    new MemoryCache(
                        c.GetRequiredService<IOptions<MemoryCacheOptions>>()),
                    c.GetRequiredService<IPubSub>()))
                .As<IMemoryCache>();
        }
    }
}
