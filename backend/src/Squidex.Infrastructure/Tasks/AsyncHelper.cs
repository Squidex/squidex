// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Squidex.Infrastructure.Tasks
{
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

            return (task1.Result, task2.Result);
        }

        public static async Task<(T1, T2, T3)> WhenAll<T1, T2, T3>(Task<T1> task1, Task<T2> task2, Task<T3> task3)
        {
            await Task.WhenAll(task1, task2, task3);

            return (task1.Result, task2.Result, task3.Result);
        }

        public static TResult Sync<TResult>(Func<Task<TResult>> func)
        {
            return TaskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }

        public static void Sync(Func<Task> func)
        {
            TaskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }

        public static IPropagatorBlock<T, T[]> CreateBatchBlock<T>(int batchSize, int timeout, GroupingDataflowBlockOptions? dataflowBlockOptions = null)
        {
            dataflowBlockOptions ??= new GroupingDataflowBlockOptions();

            var batchBlock = new BatchBlock<T>(batchSize, dataflowBlockOptions);

            var timer = new Timer(_ => batchBlock.TriggerBatch());

            var timerBlock = new TransformBlock<T, T>(value =>
            {
                timer.Change(timeout, Timeout.Infinite);

                return value;
            }, new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 1,
                CancellationToken = dataflowBlockOptions.CancellationToken,
                EnsureOrdered = dataflowBlockOptions.EnsureOrdered,
                MaxDegreeOfParallelism = 1,
                MaxMessagesPerTask = dataflowBlockOptions.MaxMessagesPerTask,
                NameFormat = dataflowBlockOptions.NameFormat,
                TaskScheduler = dataflowBlockOptions.TaskScheduler
            });

            timerBlock.LinkTo(batchBlock, new DataflowLinkOptions
            {
                PropagateCompletion = true
            });

            return DataflowBlock.Encapsulate(timerBlock, batchBlock);
        }
    }
}
