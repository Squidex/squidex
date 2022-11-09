// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;
using Squidex.Messaging;

namespace Squidex.Domain.Apps.Entities.Rules.Runner;

public sealed class RuleRunnerWorker :
    IBackgroundProcess,
    IMessageHandler<RuleRunnerRun>,
    IMessageHandler<RuleRunnerCancel>
{
    private readonly Dictionary<DomainId, Task<RuleRunnerProcessor>> processors = new Dictionary<DomainId, Task<RuleRunnerProcessor>>();
    private readonly Func<DomainId, RuleRunnerProcessor> processorFactory;
    private readonly ISnapshotStore<RuleRunnerState> snapshotStore;

    public RuleRunnerWorker(IServiceProvider serviceProvider, ISnapshotStore<RuleRunnerState> snapshotStore)
    {
        var objectFactory = ActivatorUtilities.CreateFactory(typeof(RuleRunnerProcessor), new[] { typeof(DomainId) });

        processorFactory = key =>
        {
            return (RuleRunnerProcessor)objectFactory(serviceProvider, new object[] { key });
        };

        this.snapshotStore = snapshotStore;
    }

    public async Task StartAsync(
        CancellationToken ct)
    {
        await foreach (var snapshot in snapshotStore.ReadAllAsync(ct))
        {
            await GetProcessorAsync(snapshot.Key, ct);
        }
    }

    public async Task HandleAsync(RuleRunnerRun message,
        CancellationToken ct)
    {
        var processor = await GetProcessorAsync(message.AppId, ct);

        await processor.RunAsync(message.RuleId, message.FromSnapshots, ct);
    }

    public async Task HandleAsync(RuleRunnerCancel message,
        CancellationToken ct)
    {
        var processor = await GetProcessorAsync(message.AppId, ct);

        await processor.CancelAsync();
    }

    private Task<RuleRunnerProcessor> GetProcessorAsync(DomainId appId,
        CancellationToken ct)
    {
        // Use a normal dictionary to avoid double creations.
        lock (processors)
        {
            return processors.GetOrAdd(appId, async key =>
            {
                var processor = processorFactory(key);

                await processor.LoadAsync(ct);

                return processor;
            });
        }
    }
}
