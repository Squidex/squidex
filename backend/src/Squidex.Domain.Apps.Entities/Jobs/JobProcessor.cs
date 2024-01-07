// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using NodaTime;
using Squidex.Caching;
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
    private readonly ILogger<JobProcessor> log;
    private readonly SimpleState<JobsState> state;
    private readonly ReentrantScheduler scheduler = new ReentrantScheduler(1);
    private JobRun? currentRun;

    public IClock Clock { get; set; } = SystemClock.Instance;

    public JobProcessor(DomainId ownerId,
        IEnumerable<IJobRunner> runners,
        ILocalCache localCache,
        IPersistenceFactory<JobsState> persistenceFactory,
        ILogger<JobProcessor> log)
    {
        this.ownerId = ownerId;
        this.runners = runners;
        this.localCache = localCache;
        this.log = log;

        state = new SimpleState<JobsState>(persistenceFactory, GetType(), ownerId);
    }

    public async Task LoadAsync(
        CancellationToken ct)
    {
        await state.LoadAsync(ct);

        if (state.Value.Jobs.RemoveAll(x => x.Stopped == null) > 0)
        {
            // This should actually never happen, so we log with warning.
            log.LogWarning("Removed unfinished backups for owner {ownerId} after start.", ownerId);

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

    public Task RunAsync(RefToken actor, string taskName, Dictionary<string, string> arguments,
        CancellationToken ct)
    {
        return scheduler.ScheduleAsync(async ct =>
        {
            if (currentRun != null)
            {
                throw new DomainException(T.Get("jobs.alreadyRunning"));
            }

            var runner = runners.FirstOrDefault(x => x.Name == taskName) ??
                throw new DomainException(T.Get("jobs.invalidTaskName"));

            state.Value.EnsureCanStart(runner);

            // Set the current run first to indicate that we are running a rule at the moment.
            var run = currentRun = new JobRun(state, ct)
            {
                Actor = actor,
                Job = new Job
                {
                    Id = DomainId.NewGuid(),
                    Arguments = arguments,
                    Started = default,
                    Status = JobStatus.Created,
                    TaskName = taskName
                },
                OwnerId = ownerId
            };

            log.LogInformation("Starting new backup with backup id '{backupId}' for owner {ownerId}.", run.Job.Id, ownerId);

            state.Value.Jobs.Insert(0, run.Job);
            try
            {
                await ProcessAsync(run, runner, run.CancellationToken);
            }
            finally
            {
                // Unset the run to indicate that we are done.
                currentRun.Dispose();
                currentRun = null;
            }
        }, ct);
    }

    private async Task ProcessAsync(JobRun run, IJobRunner runner,
        CancellationToken ct)
    {
        try
        {
            await SetStatusAsync(run, JobStatus.Started);

            using (localCache.StartContext())
            {
                await runner.RunAsync(run, ct);
            }

            await SetStatusAsync(run, JobStatus.Completed);
        }
        catch (OperationCanceledException)
        {
            await SetStatusAsync(run, JobStatus.Cancelled);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to run job with ID {jobId}.", run.Job.Id);

            await SetStatusAsync(run, JobStatus.Failed);
        }
    }

    private Task SetStatusAsync(JobRun run, JobStatus status)
    {
        var now = Clock.GetCurrentInstant();

        return state.UpdateAsync(_ =>
        {
            run.Job.Status = status;

            if (status == JobStatus.Started)
            {
                run.Job.Started = now;
            }
            else if (status != JobStatus.Created)
            {
                run.Job.Stopped = now;
            }

            return true;
        }, ct: default);
    }
}
