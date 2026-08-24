// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure;
using Squidex.Messaging;

namespace Squidex.Domain.Apps.Entities.Jobs;

public sealed class JobWorker :
    IMessageHandler<JobStart>,
    IMessageHandler<JobDelete>,
    IMessageHandler<JobCancel>,
    IMessageHandler<JobClear>,
    IMessageHandler<JobWakeup>
{
    private readonly Dictionary<DomainId, Task<JobProcessor>> processors = [];
    private readonly Func<DomainId, JobProcessor> processorFactory;

    public JobWorker(IServiceProvider serviceProvider)
    {
        var objectFactory = ActivatorUtilities.CreateFactory(typeof(JobProcessor), [typeof(DomainId)]);

        processorFactory = key =>
        {
            return (JobProcessor)objectFactory(serviceProvider, [key]);
        };
    }

    public async Task HandleAsync(JobStart message,
        CancellationToken ct)
    {
        var processor = await GetJobProcessorAsync(message.OwnerId);

        await processor.RunAsync(message.Request, ct);
    }

    public async Task HandleAsync(JobCancel message,
        CancellationToken ct)
    {
        var processor = await GetJobProcessorAsync(message.OwnerId);

        await processor.CancelAsync(message.TaskName);
    }

    public async Task HandleAsync(JobDelete message,
        CancellationToken ct)
    {
        var processor = await GetJobProcessorAsync(message.OwnerId);

        await processor.DeleteAsync(message.JobId);
    }

    public async Task HandleAsync(JobClear message,
        CancellationToken ct)
    {
        var processor = await GetJobProcessorAsync(message.OwnerId);

        await processor.ClearAsync();
    }

    public Task HandleAsync(JobWakeup message, CancellationToken ct)
    {
        return GetJobProcessorAsync(message.OwnerId);
    }

    private async Task<JobProcessor> GetJobProcessorAsync(DomainId appId)
    {
        Task<JobProcessor> processor;
        lock (processors)
        {
            processor = processors.GetOrAdd(appId, async key =>
            {
                var loaded = processorFactory(key);

                await loaded.LoadAsync(default);

                return loaded;
            });
        }

        try
        {
            return await processor;
        }
        catch
        {
            // A failed attempt must not stay in the cache. Loading can fail for a transient reason
            // and the jobs of the app would never run again. Only remove our own entry, so that a
            // newer successful one is not thrown away.
            lock (processors)
            {
                if (processors.TryGetValue(appId, out var current) && ReferenceEquals(current, processor))
                {
                    processors.Remove(appId);
                }
            }

            throw;
        }
    }
}
