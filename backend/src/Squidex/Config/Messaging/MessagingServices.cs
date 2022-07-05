// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps.Plans;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Runner;
using Squidex.Domain.Apps.Entities.Rules.UsageTracking;
using Squidex.Infrastructure.EventSourcing.Grains;
using Squidex.Messaging;

namespace Squidex.Config.Messaging
{
    public static class MessagingServices
    {
        public static void AddSquidexMessaging(this IServiceCollection services, IConfiguration config)
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

            services.AddMessagingTransport(config);
            services.AddMessaging(options =>
            {
                options.Routing.Add(m => m is RuleRunnerRun, "rules.run");
                options.Routing.Add(m => m is BackupStart, "backup.start");
                options.Routing.Add(m => m is BackupRestore, "backup.restore");
                options.Routing.Add(_ => true, "default");
            });

            services.AddMessaging("default", options =>
            {
                options.NumWorkers = 1;
            });

            services.AddMessaging("backup.start", options =>
            {
                options.NumWorkers = 4;
            });

            services.AddMessaging("backup.restore", options =>
            {
                options.NumWorkers = 1;
            });

            services.AddMessaging("rules.run", options =>
            {
                options.NumWorkers = 4;
            });
        }
    }
}
