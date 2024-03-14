// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Translations;
using Squidex.Messaging;

namespace Squidex.Domain.Apps.Entities.Jobs;

public sealed class DefaultJobService : IJobService, IDeleter
{
    private readonly ConcurrentDictionary<DomainId, bool> wokenUp = [];
    private readonly IMessageBus messaging;
    private readonly IEnumerable<IJobRunner> runners;
    private readonly IPersistenceFactory<JobsState> persistence;

    public DefaultJobService(IMessageBus messaging, IEnumerable<IJobRunner> runners, IPersistenceFactory<JobsState> persistence)
    {
        this.messaging = messaging;
        this.runners = runners;
        this.persistence = persistence;
    }

    Task IDeleter.DeleteAppAsync(App app, CancellationToken ct)
    {
        return messaging.PublishAsync(new JobClear(app.Id), null, ct);
    }

    public async Task DownloadAsync(Job job, Stream stream,
        CancellationToken ct = default)
    {
        Guard.NotNull(job);
        Guard.NotNull(stream);

        if (job.File == null)
        {
            throw new InvalidOperationException("Cannot download job. No file attached.");
        }

        if (job.Status != JobStatus.Completed)
        {
            throw new InvalidOperationException($"Cannot download job. Invalid status '{job.Status}'.");
        }

        // This should never happen, but just in case we remove a task, it is there to get a proper error.
        var runner = runners.FirstOrDefault(x => x.Name == job.TaskName) ??
            throw new InvalidOperationException($"Cannot find job with name '{job.TaskName}'.");

        await runner.DownloadAsync(job, stream, ct);
    }

    public async Task StartAsync(DomainId ownerId, JobRequest request,
        CancellationToken ct = default)
    {
        // This should never happen, but just in case we remove a task, it is there to get a proper error.
        var runner = runners.FirstOrDefault(x => x.Name == request.TaskName) ??
            throw new DomainException(T.Get("jobs.invalidTaskName"));

        // Wakeup the job handler to clear cancelled runs from previous incarnations of the service.
        await EnsureWakeupAsync(ownerId, ct);
        await EnsureCanRunAsync(ownerId, runner, ct);

        await messaging.PublishAsync(new JobStart(ownerId, request), null, ct);
    }

    public Task CancelAsync(DomainId ownerId, string? taskName = null,
        CancellationToken ct = default)
    {
        return messaging.PublishAsync(new JobCancel(ownerId, taskName), null, ct);
    }

    public Task DeleteJobAsync(DomainId ownerId, DomainId jobId,
        CancellationToken ct = default)
    {
        return messaging.PublishAsync(new JobDelete(ownerId, jobId), null, ct);
    }

    public async Task<List<Job>> GetJobsAsync(DomainId ownerId,
        CancellationToken ct = default)
    {
        // Wakeup the job handler to clear cancelled runs from previous incarnations of the service.
        await EnsureWakeupAsync(ownerId, default);

        var state = await GetStateAsync(ownerId, ct);

        return state.Jobs;
    }

    private async Task<JobsState> GetStateAsync(DomainId ownerId,
        CancellationToken ct = default)
    {
        var state = new SimpleState<JobsState>(persistence, typeof(JobProcessor), ownerId);

        await state.LoadAsync(ct);

        return state.Value;
    }

    private async Task EnsureCanRunAsync(DomainId ownerId, IJobRunner runner, CancellationToken ct)
    {
        var state = await GetStateAsync(ownerId, ct);

        state.EnsureCanStart(runner);
    }

    private async Task EnsureWakeupAsync(DomainId ownerId,
        CancellationToken ct)
    {
        if (wokenUp.TryAdd(ownerId, true))
        {
            await messaging.PublishAsync(new JobWakeup(ownerId), null, ct);
        }
    }
}
