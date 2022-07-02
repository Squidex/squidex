// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MassTransit;
using Squidex.Domain.Apps.Entities.Apps.Plans;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Runner;
using Squidex.Domain.Apps.Entities.Rules.UsageTracking;
using Squidex.Infrastructure.EventSourcing.Grains;

namespace Squidex.Config.Orleans
{
    public static class MasssTransitServices
    {
        public static void AddSquidexMassTransit(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingletonAs<AssetCleanupWorker>()
                .AsSelf();

            services.AddSingletonAs<BackupWorker>()
                .AsSelf();

            services.AddSingletonAs<ContentSchedulerWorker>()
                .AsSelf();

            services.AddSingletonAs<EventConsumerWorkerManager>()
                .AsSelf();

            services.AddSingletonAs<RuleRunnerWorker>()
                .AsSelf();

            services.AddSingletonAs<RuleDequeuerWorker>()
                .AsSelf();

            services.AddSingletonAs<UsageNotifierWorker>()
                .AsSelf();

            services.AddSingletonAs<UsageTrackerWorker>()
                .AsSelf();

            services.AddMassTransit(mt =>
            {
                mt.AddConsumer<EventConsumerWorkerManager>();
                mt.AddConsumer<UsageNotifierWorker>();

                mt.AddConsumer<BackupWorker>(cfg =>
                {
                    cfg.Options<JobOptions<BackupStart>>(options => options
                        .SetConcurrentJobLimit(20));
                });

                mt.AddConsumer<RuleRunnerWorker>(cfg =>
                {
                    cfg.Options<JobOptions<RuleRunnerRun>>(options => options
                        .SetConcurrentJobLimit(20));
                });

                mt.UsingInMemory();
            });
        }
    }
}
