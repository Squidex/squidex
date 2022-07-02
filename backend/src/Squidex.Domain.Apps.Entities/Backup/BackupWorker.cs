// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Hosting;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Backup
{
    public sealed class BackupWorker :
        IJobConsumer<BackupRestore>,
        IJobConsumer<BackupStart>,
        IConsumer<BackupRemove>,
        IConsumer<BackupClear>,
        IInitializable
    {
        private readonly ConcurrentDictionary<DomainId, BackupProcessor> backupProcessors = new ConcurrentDictionary<DomainId, BackupProcessor>();
        private readonly Func<DomainId, BackupProcessor> backupFactory;
        private readonly RestoreProcessor restoreProcessor;

        public BackupWorker(IServiceProvider serviceProvider)
        {
            var objectFactory = ActivatorUtilities.CreateFactory(typeof(BackupProcessor), new[] { typeof(DomainId) });

            backupFactory = key =>
            {
                return (BackupProcessor)objectFactory(serviceProvider, new object[] { key });
            };

            restoreProcessor = serviceProvider.GetRequiredService<RestoreProcessor>();
        }

        public Task InitializeAsync(
            CancellationToken ct)
        {
            return restoreProcessor.LoadAsync(ct);
        }

        public Task Run(JobContext<BackupRestore> context)
        {
            return restoreProcessor.RestoreAsync(context.Job.Url, context.Job.Actor, context.Job.NewAppName, context.CancellationToken);
        }

        public async Task Run(JobContext<BackupStart> context)
        {
            var processor = await GetBackupProcessorAsync(context.Job.AppId);

            await processor.BackupAsync(context.Job.Actor, context.CancellationToken);
        }

        public async Task Consume(ConsumeContext<BackupRemove> context)
        {
            var processor = await GetBackupProcessorAsync(context.Message.AppId);

            await processor.DeleteAsync(context.Message.Id);
        }

        public async Task Consume(ConsumeContext<BackupClear> context)
        {
            var processor = await GetBackupProcessorAsync(context.Message.AppId);

            await processor.ClearAsync();
        }

        private async Task<BackupProcessor> GetBackupProcessorAsync(DomainId appId)
        {
            var processor = backupProcessors.GetOrAdd(appId, backupFactory);

            await processor.LoadAsync(default);

            return processor;
        }
    }
}
