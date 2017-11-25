// ==========================================================================
//  LimitedConcurrencyLevelTaskScheduler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Tasks
{
    public sealed class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
    {
        [ThreadStatic]
        private static bool currentThreadIsProcessingItems;
        private readonly LinkedList<Task> tasks = new LinkedList<Task>();
        private readonly int maxDegreeOfParallelism;
        private int delegatesQueuedOrRunning;

        public override int MaximumConcurrencyLevel
        {
            get { return maxDegreeOfParallelism; }
        }

        public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
        {
            Guard.GreaterThan(maxDegreeOfParallelism, 0, nameof(maxDegreeOfParallelism));

            this.maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        protected override void QueueTask(Task task)
        {
            lock (tasks)
            {
                tasks.AddLast(task);

                if (delegatesQueuedOrRunning < maxDegreeOfParallelism)
                {
                    ++delegatesQueuedOrRunning;

                    NotifyThreadPoolOfPendingWork();
                }
            }
        }

        private void NotifyThreadPoolOfPendingWork()
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ =>
            {
                currentThreadIsProcessingItems = true;
                try
                {
                    while (true)
                    {
                        Task item;

                        lock (tasks)
                        {
                            if (tasks.Count == 0)
                            {
                                --delegatesQueuedOrRunning;
                                break;
                            }

                            item = tasks.First.Value;

                            tasks.RemoveFirst();
                        }

                        TryExecuteTask(item);
                    }
                }
                finally
                {
                    currentThreadIsProcessingItems = false;
                }
            }, null);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (!currentThreadIsProcessingItems)
            {
                return false;
            }

            if (taskWasPreviouslyQueued)
            {
                if (TryDequeue(task))
                {
                    return TryExecuteTask(task);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return TryExecuteTask(task);
            }
        }

        protected override bool TryDequeue(Task task)
        {
            lock (tasks)
            {
                return tasks.Remove(task);
            }
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            var lockTaken = false;
            try
            {
                Monitor.TryEnter(tasks, ref lockTaken);

                if (lockTaken)
                {
                    return tasks;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(tasks);
                }
            }
        }
    }
}