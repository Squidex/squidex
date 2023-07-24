// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Squidex.Infrastructure.Tasks;

public static class TaskExtensions
{
    private static readonly Action<Task> IgnoreTaskContinuation = t => { var ignored = t.Exception; };

    public static void Forget(this Task task)
    {
        if (task.IsCompleted)
        {
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            var ignored = task.Exception;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
        }
        else
        {
#pragma warning disable MA0134 // Observe result of async calls
            task.ContinueWith(
                IgnoreTaskContinuation,
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted |
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
#pragma warning restore MA0134 // Observe result of async calls
        }
    }

    public static async Task<T> WithCancellation<T>(this Task<T> task,
        CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

        await using (cancellationToken.Register(state =>
        {
            ((TaskCompletionSource<object>)state!).TrySetResult(null!);
        },
        tcs))
        {
            var resultTask = await Task.WhenAny(task, tcs.Task);
            if (resultTask == tcs.Task)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            return await task;
        }
    }

    public static async IAsyncEnumerable<T> Buffered<T>(this IAsyncEnumerable<T> source, int capacity,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var bufferChannel = Channel.CreateBounded<T>(new BoundedChannelOptions(capacity)
        {
            SingleWriter = true,
            SingleReader = true,
        });

        using var bufferCompletion = new CancellationTokenSource();

        var producer = Task.Run(async () =>
        {
            try
            {
                await foreach (var item in source.WithCancellation(bufferCompletion.Token).ConfigureAwait(false))
                {
                    await bufferChannel.Writer.WriteAsync(item, bufferCompletion.Token).ConfigureAwait(false);
                }
            }
            catch (ChannelClosedException)
            {
                // Ignore
            }
            catch (OperationCanceledException)
            {
                // Ignore
            }
            finally
            {
                bufferChannel.Writer.TryComplete();
            }
        }, bufferCompletion.Token);

        try
        {
            await foreach (T item in bufferChannel.Reader.ReadAllAsync(ct).ConfigureAwait(false))
            {
                yield return item;

                ct.ThrowIfCancellationRequested();
            }

            await producer.ConfigureAwait(false); // Propagate possible source error
        }
        finally
        {
            if (!producer.IsCompleted)
            {
                bufferCompletion.Cancel();
                bufferChannel.Writer.TryComplete();

                await Task.WhenAny(producer).ConfigureAwait(false);
            }
        }
    }
}
