// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Tasks
{
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

        public void AddTask(SchedulerTask task)
        {
            if (pendingTasks >= 1)
            {
                // If we already in a tasks we just queue it with the semaphore.
                RunTask(task, default).Forget();
                return;
            }

            tasks ??= new List<SchedulerTask>(1);
            tasks.Add(task);
        }

        public async ValueTask RunAsync(
            CancellationToken ct = default)
        {
            if (tasks == null || tasks.Count == 0)
            {
                return;
            }

            // Use the value to indicate that the task have been started.
            pendingTasks = 1;

            RunTasks(tasks, ct).AsTask().Forget();

            await tcs.Task;
        }

        private async ValueTask RunTasks(List<SchedulerTask>? validationTasks,
            CancellationToken ct)
        {
            if (validationTasks == null || validationTasks.Count == 0)
            {
                tcs.TrySetResult(true);
                return;
            }

            if (validationTasks.Count == 1)
            {
                await RunTask(validationTasks[0], ct);
                return;
            }

            var runningTasks = new List<Task>();

            foreach (var validationTask in validationTasks)
            {
                runningTasks.Add(RunTask(validationTask, ct));
            }

            await Task.WhenAll(runningTasks);
        }

        private async Task RunTask(SchedulerTask task,
            CancellationToken ct)
        {
            try
            {
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
                if (Interlocked.Decrement(ref pendingTasks) <= 1)
                {
                    tcs.TrySetResult(true);
                }
            }
        }
    }
}
