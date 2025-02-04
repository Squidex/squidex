﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Text.Json;
using Squidex.AI;
using Squidex.Domain.Apps.Core.Subscriptions;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Billing;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Indexes;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Runner;
using Squidex.Domain.Apps.Entities.Rules.UsageTracking;
using Squidex.Hosting.Configuration;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.EventSourcing.Consume;
using Squidex.Messaging;
using Squidex.Messaging.Implementation;
using Squidex.Messaging.Implementation.Null;
using Squidex.Messaging.Implementation.Scheduler;

namespace Squidex.Config.Messaging;

public static class MessagingServices
{
    public static void AddSquidexMessaging(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<MessagingOptions>(config,
            "messaging");

        var channelBackupRestore = new ChannelName("backup.restore");
        var channelBackupStart = new ChannelName("backup.start");
        var channelFallback = new ChannelName("default");
        var channelRules = new ChannelName("rules.run");
        var isRandomName = config.GetValue<bool>("clustering:randomName");
        var isWorker = config.GetValue<bool>("clustering:worker");

        if (isWorker)
        {
            services.AddAI()
                .AddCleaner();

            services.AddSingletonAs<AssetCleanupProcess>()
                .AsSelf();

            services.AddSingletonAs<ContentSchedulerProcess>()
                .AsSelf();

            services.AddSingletonAs<RuleDequeuerWorker>()
                .AsSelf();

            services.AddSingletonAs<EventConsumerWorker>()
                .AsSelf().As<IMessageHandler>();

            services.AddSingletonAs<JobWorker>()
                .AsSelf().As<IMessageHandler>();

            services.AddSingletonAs<UsageNotifierWorker>()
                .AsSelf().As<IMessageHandler>();

            services.AddSingletonAs<UsageTrackerWorker>()
                .AsSelf().As<IMessageHandler>();
        }

        if (isRandomName)
        {
            services.AddSingletonAs<RandomInstanceNameProvider>()
                .As<IInstanceNameProvider>();
        }

        services.AddSingletonAs<BackupJob>()
            .As<IJobRunner>();

        services.AddSingletonAs<RestoreJob>()
            .As<IJobRunner>();

        services.AddSingletonAs<RuleRunnerJob>()
            .As<IJobRunner>();

        services.AddSingletonAs<CreateIndexJob>()
            .As<IJobRunner>();

        services.AddSingletonAs<DropIndexJob>()
            .As<IJobRunner>();

        services.AddSingleton<IMessagingSerializer>(c =>
            new SystemTextJsonMessagingSerializer(c.GetRequiredService<JsonSerializerOptions>()));

        services.AddSingletonAs<SubscriptionPublisher>()
            .As<IEventConsumer>();

        services.AddSingletonAs<DefaultJobService>()
            .As<IJobService>().As<IDeleter>();

        services.AddMessaging()
            .AddSquidexTransport(config)
            .AddSubscriptions(!isWorker)
            .AddReplicatedCache(true, options =>
            {
                options.TransportSelector = (transport, _) => transport.First(x => x is not NullTransport);
            })
            .Configure(options =>
            {
                options.Routing.Add(m => m is JobStart r && r.Request.TaskName == BackupJob.TaskName, channelBackupStart);
                options.Routing.Add(m => m is JobStart r && r.Request.TaskName == RestoreJob.TaskName, channelBackupRestore);
                options.Routing.Add(m => m is JobStart r && r.Request.TaskName == RuleRunnerJob.TaskName, channelRules);
                options.Routing.AddFallback(channelFallback);
            })
            .AddChannel(channelBackupStart, isWorker, options =>
            {
                options.Timeout = TimeSpan.FromHours(4);
                options.Scheduler = new ParallelScheduler(4);
                options.LogMessage = x => true;
            })
            .AddChannel(channelBackupRestore, isWorker, options =>
            {
                options.Timeout = TimeSpan.FromHours(24);
                options.Scheduler = InlineScheduler.Instance;
                options.LogMessage = x => true;
            })
            .AddChannel(channelRules, isWorker, options =>
            {
                options.Scheduler = new ParallelScheduler(4);
                options.LogMessage = x => true;
            })
            .AddChannel(channelFallback, isWorker, options =>
            {
                options.Scheduler = InlineScheduler.Instance;
            });
    }

    public static MessagingBuilder AddSquidexTransport(this MessagingBuilder builder, IConfiguration config)
    {
        var type = config.GetValue<string>("messaging:type");

        if (string.Equals(type, "Sql", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.Equals(config.GetValue<string>("store:type"), "Sql", StringComparison.OrdinalIgnoreCase))
            {
                throw new ConfigurationException(
                    new ConfigurationError(
                        "Sql messaging transport is only allowed, when 'store:type' is also set to 'Sql'.",
                        "messaging:type"));
            }

            return builder.AddSquidexEntityFrameworkTransport(config);
        }

        return builder.AddTransport(config);
    }
}
