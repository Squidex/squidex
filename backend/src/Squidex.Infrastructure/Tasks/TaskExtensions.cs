// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Tasks
{
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

        public static Func<TInput, TOutput> ToDefault<TInput, TOutput>(this Action<TInput> action)
        {
            Guard.NotNull(action, nameof(action));

            return x =>
            {
                action(x);

                return default!;
            };
        }

        public static Func<TInput, Task<TOutput>> ToDefault<TInput, TOutput>(this Func<TInput, Task> action)
        {
            Guard.NotNull(action, nameof(action));

            return async x =>
            {
                await action(x);

                return default!;
            };
        }

        public static Func<TInput, Task<TOutput>> ToAsync<TInput, TOutput>(this Func<TInput, TOutput> action)
        {
            Guard.NotNull(action, nameof(action));

            return x =>
            {
                var result = action(x);

                return Task.FromResult(result);
            };
        }

        public static Func<TInput, Task> ToAsync<TInput>(this Action<TInput> action)
        {
            return x =>
            {
                action(x);

                return Task.CompletedTask;
            };
        }

        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

            using (cancellationToken.Register(state =>
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
    }
}
