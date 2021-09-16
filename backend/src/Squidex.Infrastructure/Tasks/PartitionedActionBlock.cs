// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Squidex.Infrastructure.Tasks
{
    public sealed class PartitionedActionBlock<TInput> : ITargetBlock<TInput>
    {
        private readonly ITargetBlock<TInput> distributor;
        private readonly ActionBlock<TInput>[] workers;

        public Task Completion
        {
            get => Task.WhenAll(workers.Select(x => x.Completion));
        }

        public PartitionedActionBlock(Func<TInput, Task> action, Func<TInput, long> partitioner)
            : this(action, partitioner, new ExecutionDataflowBlockOptions())
        {
        }

        public PartitionedActionBlock(Func<TInput, Task> action, Func<TInput, long> partitioner, ExecutionDataflowBlockOptions dataflowBlockOptions)
        {
            Guard.NotNull(action, nameof(action));
            Guard.NotNull(partitioner, nameof(partitioner));
            Guard.NotNull(dataflowBlockOptions, nameof(dataflowBlockOptions));
            Guard.GreaterThan(dataflowBlockOptions.MaxDegreeOfParallelism, 1, nameof(dataflowBlockOptions.MaxDegreeOfParallelism));

            workers = new ActionBlock<TInput>[dataflowBlockOptions.MaxDegreeOfParallelism];

            for (var i = 0; i < dataflowBlockOptions.MaxDegreeOfParallelism; i++)
            {
                workers[i] = new ActionBlock<TInput>(action, new ExecutionDataflowBlockOptions()
                {
                    BoundedCapacity = dataflowBlockOptions.BoundedCapacity,
                    CancellationToken = dataflowBlockOptions.CancellationToken,
                    MaxDegreeOfParallelism = 1,
                    MaxMessagesPerTask = 1,
                    TaskScheduler = dataflowBlockOptions.TaskScheduler,
                });
            }

            distributor = new ActionBlock<TInput>(async input =>
            {
                try
                {
                    var partition = Math.Abs(partitioner(input)) % workers.Length;

                    await workers[partition].SendAsync(input);
                }
                catch (OperationCanceledException ex)
                {
                    // Dataflow swallows operation cancelled exception.
                    throw new AggregateException(ex);
                }
            }, new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 1,
                MaxDegreeOfParallelism = 1,
                MaxMessagesPerTask = 1
            });

            LinkCompletion();
        }

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, TInput messageValue, ISourceBlock<TInput>? source, bool consumeToAccept)
        {
            return distributor.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
        }

        public void Complete()
        {
            distributor.Complete();
        }

        public void Fault(Exception exception)
        {
            distributor.Fault(exception);
        }

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        private async void LinkCompletion()
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            try
            {
                await distributor.Completion.ConfigureAwait(false);
            }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            catch
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
            {
                // we do not want to change the stacktrace of the exception.
            }

            if (distributor.Completion.IsFaulted && distributor.Completion.Exception != null)
            {
                foreach (var worker in workers)
                {
                    ((IDataflowBlock)worker).Fault(distributor.Completion.Exception);
                }
            }
            else
            {
                foreach (var worker in workers)
                {
                    worker.Complete();
                }
            }
        }
    }
}
