// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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

        public SingleThreadedDispatcher(int capacity = 1)
        {
            var options = new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = capacity,
                MaxMessagesPerTask = 1,
                MaxDegreeOfParallelism = 1
            };

            block = new ActionBlock<Func<Task>>(Handle, options);
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
