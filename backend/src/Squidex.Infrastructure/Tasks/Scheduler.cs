// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Tasks;

#pragma warning disable MA0048 // File name must match type name
public delegate Task SchedulerTask(CancellationToken ct);
#pragma warning restore MA0048 // File name must match type name

public sealed class Scheduler
{
    private const int SpecialStateStartedOrDone = 1;
    private const int SpecialStateCompleted = -1;
    private readonly TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
    private readonly SemaphoreSlim semaphore;
    private List<SchedulerTask>? tasks;
    private int pendingTasks;

    public Scheduler(int maxDegreeOfParallelism = 0)
    {
        if (maxDegreeOfParallelism <= 0)
        {
            maxDegreeOfParallelism = Environment.ProcessorCount * 2;
        }

        semaphore = new SemaphoreSlim(maxDegreeOfParallelism);
    }

    public void Schedule(SchedulerTask task)
    {
        if (pendingTasks <= SpecialStateCompleted)
        {
            // Already completed.
            return;
        }

        if (pendingTasks >= 1)
        {
            // If we already in a tasks we just queue it with the semaphore.
            ScheduleTasks([task], default);
            return;
        }

        tasks ??= new List<SchedulerTask>(1);
        tasks.Add(task);
    }

    public async ValueTask CompleteAsync(
        CancellationToken ct = default)
    {
        // Do not allow another completion call.
        if (tasks == null || pendingTasks <= SpecialStateCompleted)
        {
            return;
        }

        // Use the value to indicate that the task have been started.
        pendingTasks = SpecialStateStartedOrDone;
        try
        {
            ScheduleTasks(tasks, ct);
            await tcs.Task;
        }
        finally
        {
            pendingTasks = SpecialStateCompleted;
        }
    }

    private void ScheduleTasks(IReadOnlyCollection<SchedulerTask> taskToSchedule,
        CancellationToken ct)
    {
        // Increment the pending tasks once, so we avoid issues when the tasks are executed sequentially.
        Interlocked.Add(ref pendingTasks, taskToSchedule.Count);

        foreach (var task in taskToSchedule)
        {
            ScheduleTask(task, ct).Forget();
        }
    }

    private async Task ScheduleTask(SchedulerTask task,
        CancellationToken ct)
    {
        try
        {
            // Use the semaphore to reduce degree of parallelization.
            await semaphore.WaitAsync(ct);
            await task(ct);
        }
        catch
        {
            return;
        }
        finally
        {
            semaphore.Release();

            if (Interlocked.Decrement(ref pendingTasks) <= SpecialStateStartedOrDone)
            {
                tcs.TrySetResult(true);
            }
        }
    }
}
