// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using Squidex.Domain.Apps.Core.Subscriptions;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Runner;
using Squidex.Domain.Apps.Entities.Rules.UsageTracking;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.EventSourcing.Consume;
using Squidex.Messaging;
using Squidex.Messaging.Implementation;
using Squidex.Messaging.Implementation.Null;
using Squidex.Messaging.Implementation.Scheduler;
using Squidex.Messaging.Subscriptions;

namespace Squidex.Config.Messaging;

public static class MessagingServices
{
    public static void AddSquidexMessaging(this IServiceCollection services, IConfiguration config)
    {
        var channelBackupRestore = new ChannelName("backup.restore");
        var channelBackupStart = new ChannelName("backup.start");
        var channelFallback = new ChannelName("default");
        var channelRules = new ChannelName("rules.run");
        var isCaching = config.GetValue<bool>("caching:replicated:enable");
        var isWorker = config.GetValue<bool>("clustering:worker");

        if (isWorker)
        {
            services.AddSingletonAs<AssetCleanupProcess>()
                .AsSelf();

            services.AddSingletonAs<ContentSchedulerProcess>()
                .AsSelf();

            services.AddSingletonAs<RuleDequeuerWorker>()
                .AsSelf();

            services.AddSingletonAs<EventConsumerWorker>()
                .AsSelf().As<IMessageHandler>();

            services.AddSingletonAs<RuleRunnerWorker>()
                .AsSelf().As<IMessageHandler>();

            services.AddSingletonAs<BackupWorker>()
                .AsSelf().As<IMessageHandler>();

            services.AddSingletonAs<UsageNotifierWorker>()
                .AsSelf().As<IMessageHandler>();

            services.AddSingletonAs<UsageTrackerWorker>()
                .AsSelf().As<IMessageHandler>();
        }

        services.AddSingleton<IMessagingSerializer>(c =>
            new SystemTextJsonMessagingSerializer(c.GetRequiredService<JsonSerializerOptions>()));

        services.AddSingletonAs<SubscriptionPublisher>()
            .As<IEventConsumer>();

        services.AddSingletonAs<EventMessageEvaluator>()
            .As<IMessageEvaluator>();

        services.AddReplicatedCacheMessaging(isCaching, options =>
        {
            options.TransportSelector = (transport, _) => transport.First(x => x is NullTransport != isCaching);
        });

        services.Configure<SubscriptionOptions>(options =>
        {
            options.SendMessagesToSelf = false;
        });

        services.AddMessagingSubscriptions();
        services.AddMessagingTransport(config);
        services.AddMessaging(options =>
        {
            options.Routing.Add(m => m is RuleRunnerRun, channelRules);
            options.Routing.Add(m => m is BackupStart, channelBackupStart);
            options.Routing.Add(m => m is BackupRestore, channelBackupRestore);
            options.Routing.AddFallback(channelFallback);
        });

        services.AddMessaging(channelBackupStart, isWorker, options =>
        {
            options.Timeout = TimeSpan.FromHours(4);
            options.Scheduler = new ParallelScheduler(4);
            options.LogMessage = x => true;
        });

        services.AddMessaging(channelBackupRestore, isWorker, options =>
        {
            options.Timeout = TimeSpan.FromHours(24);
            options.Scheduler = InlineScheduler.Instance;
            options.LogMessage = x => true;
        });

        services.AddMessaging(channelRules, isWorker, options =>
        {
            options.Scheduler = new ParallelScheduler(4);
            options.LogMessage = x => true;
        });

        services.AddMessaging(channelFallback, isWorker, options =>
        {
            options.Scheduler = InlineScheduler.Instance;
        });
    }
}
