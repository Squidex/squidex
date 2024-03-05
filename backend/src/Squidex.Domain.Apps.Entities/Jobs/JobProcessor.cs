// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using NodaTime;
using Squidex.Caching;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities.Collaboration;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Entities.Jobs;

public sealed class JobProcessor
{
    private readonly DomainId ownerId;
    private readonly IEnumerable<IJobRunner> runners;
    private readonly ILocalCache localCache;
    private readonly ICollaborationService collaboration;
    private readonly IUrlGenerator urlGenerator;
    private readonly ILogger<JobProcessor> log;
    private readonly SimpleState<JobsState> state;
    private readonly ReentrantScheduler scheduler = new ReentrantScheduler(1);
    private JobRunContext? currentRun;

    public IClock Clock { get; init; } = SystemClock.Instance;

    public JobProcessor(DomainId ownerId,
        IEnumerable<IJobRunner> runners,
        ILocalCache localCache,
        ICollaborationService collaboration,
        IPersistenceFactory<JobsState> persistenceFactory,
        IUrlGenerator urlGenerator,
        ILogger<JobProcessor> log)
    {
        this.ownerId = ownerId;
        this.runners = runners;
        this.localCache = localCache;
        this.collaboration = collaboration;
        this.urlGenerator = urlGenerator;
        this.log = log;

        state = new SimpleState<JobsState>(persistenceFactory, GetType(), ownerId);
    }

    public async Task LoadAsync(
        CancellationToken ct)
    {
        await state.LoadAsync(ct);

        var pending = state.Value.Jobs.Where(x => x.Stopped == null);

        if (pending.Any())
        {
            // This should actually never happen, so we log with warning.
            log.LogWarning("Removed unfinished jobs for owner {ownerId} after start.", ownerId);

            foreach (var job in pending.ToList())
            {
                var runner = runners.FirstOrDefault(x => x.Name == job.TaskName);

                if (runner != null)
                {
                    await runner.CleanupAsync(job);
                }

                state.Value.Jobs.Remove(job);
            }

            await state.WriteAsync(ct);
        }
    }

    public Task DeleteAsync(DomainId jobId)
    {
        return scheduler.ScheduleAsync(async _ =>
        {
            log.LogInformation("Clearing jobs for owner {ownerId}.", ownerId);

            var job = state.Value.Jobs.Find(x => x.Id == jobId);

            if (job == null)
            {
                return;
            }

            var runner = runners.FirstOrDefault(x => x.Name == job.TaskName);

            if (runner != null)
            {
                await runner.CleanupAsync(job);
            }

            await state.UpdateAsync(state => state.Jobs.RemoveAll(x => x.Id == jobId) > 0, ct: default);
        }, default);
    }

    public Task ClearAsync()
    {
        return scheduler.ScheduleAsync(async _ =>
        {
            log.LogInformation("Clearing jobs for owner {ownerId}.", ownerId);

            foreach (var job in state.Value.Jobs)
            {
                var runner = runners.FirstOrDefault(x => x.Name == job.TaskName);

                if (runner != null)
                {
                    await runner.CleanupAsync(job);
                }
            }

            await state.ClearAsync(default);
        }, default);
    }

    public Task CancelAsync(string? taskName)
    {
        // Ensure that only one thread is accessing the current state at a time.
        return scheduler.Schedule(() =>
        {
            if (taskName == null || currentRun?.Job.TaskName == taskName)
            {
                currentRun?.Cancel();
            }
        });
    }

    public Task RunAsync(JobRequest request,
        CancellationToken ct)
    {
        return scheduler.ScheduleAsync(async ct =>
        {
            if (currentRun != null)
            {
                throw new DomainException(T.Get("jobs.alreadyRunning"));
            }

            var runner = runners.FirstOrDefault(x => x.Name == request.TaskName) ??
                throw new DomainException(T.Get("jobs.invalidTaskName"));

            state.Value.EnsureCanStart(runner);

            // Set the current run first to indicate that we are running a rule at the moment.
            var context = currentRun = new JobRunContext(state, Clock, ct)
            {
                Actor = request.Actor,
                Job = new Job
                {
                    Id = DomainId.NewGuid(),
                    Arguments = request.Arguments,
                    Description = request.TaskName,
                    Started = default,
                    Status = JobStatus.Created,
                    TaskName = request.TaskName
                },
                OwnerId = ownerId
            };

            log.LogInformation("Starting new backup with backup id '{backupId}' for owner {ownerId}.", context.Job.Id, ownerId);

            state.Value.Jobs.Insert(0, context.Job);
            try
            {
                await ProcessAsync(context, runner, context.CancellationToken);

                await NotifyAsync(request, T.Get("jobs.notifySuccess", new { job = context.Job.Description }));
            }
            catch
            {
                await NotifyAsync(request, T.Get("jobs.notifyFailed", new { job = context.Job.Description }));
                throw;
            }
            finally
            {
                // Unset the run to indicate that we are done.
                currentRun.Dispose();
                currentRun = null;
            }
        }, ct);
    }

    private async Task NotifyAsync(JobRequest request, string text)
    {
        if (request.AppId == null || request.Actor.IsClient)
        {
            return;
        }

        var notificationText = text;
        var notificationUrl = new Uri(urlGenerator.JobsUI(request.AppId));

        await collaboration.NotifyAsync(request.Actor.Identifier, notificationText, request.Actor, notificationUrl, false, default);
    }

    private async Task ProcessAsync(JobRunContext context, IJobRunner runner,
        CancellationToken ct)
    {
        try
        {
            await SetStatusAsync(context, JobStatus.Started);

            using (localCache.StartContext())
            {
                await runner.RunAsync(context, ct);
            }

            await SetStatusAsync(context, JobStatus.Completed);
        }
        catch (OperationCanceledException)
        {
            await SetStatusAsync(context, JobStatus.Cancelled);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to run job with ID {jobId}.", context.Job.Id);

            await SetStatusAsync(context, JobStatus.Failed);
        }
    }

    private Task SetStatusAsync(JobRunContext context, JobStatus status)
    {
        var now = Clock.GetCurrentInstant();

        return state.UpdateAsync(_ =>
        {
            context.Job.Status = status;

            if (status == JobStatus.Started)
            {
                context.Job.Started = now;
            }
            else if (status != JobStatus.Created)
            {
                context.Job.Stopped = now;
            }

            return true;
        }, ct: default);
    }
}
