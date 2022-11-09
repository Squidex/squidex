// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks.Dataflow;

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
            task.ContinueWith(
                IgnoreTaskContinuation,
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted |
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
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

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
    public static async void BidirectionalLinkTo<T>(this ISourceBlock<T> source, ITargetBlock<T> target)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
    {
        source.LinkTo(target, new DataflowLinkOptions
        {
            PropagateCompletion = true
        });

        try
        {
            await target.Completion.ConfigureAwait(false);
        }
        catch
        {
            // We do not want to change the stacktrace of the exception.
            return;
        }

        if (target.Completion.IsFaulted && target.Completion.Exception != null)
        {
            source.Fault(target.Completion.Exception.Flatten());
        }
    }
}
