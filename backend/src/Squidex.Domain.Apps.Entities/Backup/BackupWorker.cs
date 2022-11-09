// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Messaging;

namespace Squidex.Domain.Apps.Entities.Backup;

public sealed class BackupWorker :
    IMessageHandler<BackupRestore>,
    IMessageHandler<BackupStart>,
    IMessageHandler<BackupDelete>,
    IMessageHandler<BackupClear>,
    IInitializable
{
    private readonly Dictionary<DomainId, Task<BackupProcessor>> backupProcessors = new Dictionary<DomainId, Task<BackupProcessor>>();
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

    public Task HandleAsync(BackupRestore message,
        CancellationToken ct)
    {
        return restoreProcessor.RestoreAsync(message.Url, message.Actor, message.NewAppName, ct);
    }

    public async Task HandleAsync(BackupStart message,
        CancellationToken ct)
    {
        var processor = await GetBackupProcessorAsync(message.AppId);

        await processor.BackupAsync(message.Actor, ct);
    }

    public async Task HandleAsync(BackupDelete message,
        CancellationToken ct)
    {
        var processor = await GetBackupProcessorAsync(message.AppId);

        await processor.DeleteAsync(message.Id);
    }

    public async Task HandleAsync(BackupClear message,
        CancellationToken ct)
    {
        var processor = await GetBackupProcessorAsync(message.AppId);

        await processor.ClearAsync();
    }

    private Task<BackupProcessor> GetBackupProcessorAsync(DomainId appId)
    {
        lock (backupProcessors)
        {
            return backupProcessors.GetOrAdd(appId, async key =>
            {
                var processor = backupFactory(key);

                await processor.LoadAsync(default);

                return processor;
            });
        }
    }
}
