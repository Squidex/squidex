// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Channels;

namespace Squidex.Infrastructure.Tasks;

public static class AsyncHelper
{
    private static readonly TaskFactory TaskFactory = new
        TaskFactory(CancellationToken.None,
            TaskCreationOptions.None,
            TaskContinuationOptions.None,
            TaskScheduler.Default);

    public static async Task<(T1, T2)> WhenAll<T1, T2>(Task<T1> task1, Task<T2> task2)
    {
        await Task.WhenAll(task1, task2);

#pragma warning disable MA0042 // Do not use blocking calls in an async method
        return (task1.Result, task2.Result);
#pragma warning restore MA0042 // Do not use blocking calls in an async method
    }

    public static async Task<(T1, T2, T3)> WhenAll<T1, T2, T3>(Task<T1> task1, Task<T2> task2, Task<T3> task3)
    {
        await Task.WhenAll(task1, task2, task3);

#pragma warning disable MA0042 // Do not use blocking calls in an async method
        return (task1.Result, task2.Result, task3.Result);
#pragma warning restore MA0042 // Do not use blocking calls in an async method
    }

    public static TResult Sync<TResult>(Func<Task<TResult>> func)
    {
        return TaskFactory
            .StartNew(func).Unwrap()
            .GetAwaiter()
            .GetResult();
    }

    public static void Sync(Func<Task> func)
    {
        TaskFactory
            .StartNew(func).Unwrap()
            .GetAwaiter()
            .GetResult();
    }

    public static void Batch<TIn>(this Channel<object> source, Channel<object> target, int batchSize, int timeout,
        CancellationToken ct = default)
    {
        Task.Run(async () =>
        {
            var batch = new List<TIn>(batchSize);

            // Just a marker object to force sending out new batches.
            var force = new object();

            await using var timer = new Timer(_ => source.Writer.TryWrite(force));

            async Task TrySendAsync()
            {
                if (batch.Count > 0)
                {
                    await target.Writer.WriteAsync(batch, ct);

                    // Create a new batch, because the value is shared and might be processes by another concurrent task.
                    batch = new List<TIn>();
                }
            }

            // Exceptions usually that the process was stopped and the channel closed, therefore we do not catch them.
            await foreach (var item in source.Reader.ReadAllAsync(ct))
            {
                if (ReferenceEquals(item, force))
                {
                    // Our item is the marker object from the timer.
                    await TrySendAsync();
                }
                else if (item is TIn typed)
                {
                    // The timeout just with the last event and should push events out if no further events are received.
                    timer.Change(timeout, Timeout.Infinite);

                    batch.Add(typed);

                    if (batch.Count >= batchSize)
                    {
                        await TrySendAsync();
                    }
                }
            }

            await TrySendAsync();
        }, ct).ContinueWith(x => target.Writer.TryComplete(x.Exception));
    }
}
