// ==========================================================================
//  Actor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Squidex.Infrastructure.Tasks
{
    public sealed class SingleThreadedDispatcher
    {
        private readonly ActionBlock<Func<Task>> block;
        private bool isStopped;

        public SingleThreadedDispatcher(int capacity = 1, TaskScheduler scheduler = null)
        {
            var options = new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = capacity,
                MaxMessagesPerTask = -1,
                MaxDegreeOfParallelism = 1,
                TaskScheduler = scheduler ?? TaskScheduler.Default
            };

            block = new ActionBlock<Func<Task>>(Handle, options);
        }

        public Task DispatchAndUnwrapAsync(Func<Task> action)
        {
            Guard.NotNull(action, nameof(action));

            var cts = new TaskCompletionSource<bool>();

            block.SendAsync(async () =>
            {
                try
                {
                    await action();

                    cts.SetResult(true);
                }
                catch (Exception ex)
                {
                    cts.TrySetException(ex);
                }
            });

            return cts.Task;
        }

        public Task DispatchAndUnwrapAsync(Action action)
        {
            Guard.NotNull(action, nameof(action));

            return DispatchAndUnwrapAsync(() =>
            {
                action();

                return TaskHelper.Done;
            });
        }

        public Task DispatchAsync(Func<Task> action)
        {
            Guard.NotNull(action, nameof(action));

            return block.SendAsync(action);
        }

        public Task DispatchAsync(Action action)
        {
            Guard.NotNull(action, nameof(action));

            return block.SendAsync(() => { action(); return TaskHelper.Done; });
        }

        public async Task StopAndWaitAsync()
        {
            await DispatchAsync(() =>
            {
                isStopped = true;

                block.Complete();
            });

            await block.Completion;
        }

        private Task Handle(Func<Task> action)
        {
            if (isStopped)
            {
                return TaskHelper.Done;
            }

            return action();
        }
    }
}
