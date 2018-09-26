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
using Squidex.Infrastructure.Reflection;

namespace Squidex.Infrastructure.Tasks
{
    public class PartitionedActionBlock<TInput> : ITargetBlock<TInput>
    {
        private readonly ITargetBlock<TInput> distributor;
        private readonly ActionBlock<TInput>[] workers;

        public Task Completion
        {
            get { return Task.WhenAll(workers.Select(x => x.Completion)); }
        }

        public PartitionedActionBlock(Action<TInput> action, Func<TInput, int> partitioner)
            : this (action?.ToAsync(), partitioner, new ExecutionDataflowBlockOptions())
        {
        }

        public PartitionedActionBlock(Func<TInput, Task> action, Func<TInput, int> partitioner)
            : this(action, partitioner, new ExecutionDataflowBlockOptions())
        {
        }

        public PartitionedActionBlock(Action<TInput> action, Func<TInput, int> partitioner, ExecutionDataflowBlockOptions dataflowBlockOptions)
            : this(action?.ToAsync(), partitioner, dataflowBlockOptions)
        {
        }

        public PartitionedActionBlock(Func<TInput, Task> action, Func<TInput, int> partitioner, ExecutionDataflowBlockOptions dataflowBlockOptions)
        {
            Guard.NotNull(action, nameof(action));
            Guard.NotNull(partitioner, nameof(partitioner));
            Guard.NotNull(dataflowBlockOptions, nameof(dataflowBlockOptions));
            Guard.GreaterThan(dataflowBlockOptions.MaxDegreeOfParallelism, 1, nameof(dataflowBlockOptions.MaxDegreeOfParallelism));

            workers = new ActionBlock<TInput>[dataflowBlockOptions.MaxDegreeOfParallelism];

            for (var i = 0; i < dataflowBlockOptions.MaxDegreeOfParallelism; i++)
            {
                var workerOption = SimpleMapper.Map(dataflowBlockOptions, new ExecutionDataflowBlockOptions());

                workerOption.MaxDegreeOfParallelism = 1;
                workerOption.MaxMessagesPerTask = 1;

                workers[i] = new ActionBlock<TInput>(action, workerOption);
            }

            var distributorOption = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 1,
                MaxMessagesPerTask = 1,
                BoundedCapacity = 1
            };

            distributor = new ActionBlock<TInput>(x =>
            {
                var partition = Math.Abs(partitioner(x)) % workers.Length;

                return workers[partition].SendAsync(x);
            }, distributorOption);

            distributor.Completion.ContinueWith(x =>
            {
                foreach (var worker in workers)
                {
                    worker.Complete();
                }
            });
        }

        public DataflowMessageStatus OfferMessage(DataflowMessageHeader messageHeader, TInput messageValue, ISourceBlock<TInput> source, bool consumeToAccept)
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
    }
}
