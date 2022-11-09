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
        if (pendingTasks < 0)
        {
            // Already completed.
            return;
        }

        if (pendingTasks >= 1)
        {
            // If we already in a tasks we just queue it with the semaphore.
            ScheduleTask(task, default).Forget();
            return;
        }

        tasks ??= new List<SchedulerTask>(1);
        tasks.Add(task);
    }

    public async ValueTask CompleteAsync(
        CancellationToken ct = default)
    {
        if (tasks == null || tasks.Count == 0)
        {
            return;
        }

        // Use the value to indicate that the task have been started.
        pendingTasks = 1;
        try
        {
            RunTasks(ct).AsTask().Forget();

            await tcs.Task;
        }
        finally
        {
            pendingTasks = -1;
        }
    }

    private async ValueTask RunTasks(
        CancellationToken ct)
    {
        // If nothing needs to be done, we can just stop here.
        if (tasks == null || tasks.Count == 0)
        {
            tcs.TrySetResult(true);
            return;
        }

        // Quick check to avoid the allocation of the list.
        if (tasks.Count == 1)
        {
            await ScheduleTask(tasks[0], ct);
            return;
        }

        var runningTasks = new List<Task>();

        foreach (var validationTask in tasks)
        {
            runningTasks.Add(ScheduleTask(validationTask, ct));
        }

        await Task.WhenAll(runningTasks);
    }

    private async Task ScheduleTask(SchedulerTask task,
        CancellationToken ct)
    {
        try
        {
            // Use the interlock to reduce degree of parallelization.
            Interlocked.Increment(ref pendingTasks);

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

            if (Interlocked.Decrement(ref pendingTasks) <= 1)
            {
                tcs.TrySetResult(true);
            }
        }
    }
}
