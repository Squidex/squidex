// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Xunit;

namespace Squidex.Infrastructure.Tasks
{
    public class PartitionedActionBlockTests
    {
        [Fact]
        public async Task Should_propagate_in_order()
        {
            var random = new Random();
            var partitions = 10;

            var lists = new List<int>[partitions];

            for (var i = 0; i < partitions; i++)
            {
                lists[i] = new List<int>();
            }

            var block = new PartitionedActionBlock<(int P, int V)>(x =>
            {
                random.Next(10);

                lists[x.P].Add(x.V);

                return Task.CompletedTask;
            }, x => x.P, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 100,
                MaxMessagesPerTask = 1,
                BoundedCapacity = 100
            });

            for (var i = 0; i < partitions; i++)
            {
                for (var j = 0; j < 10; j++)
                {
                    await block.SendAsync((i, j));
                }
            }

            block.Complete();

            await block.Completion;

            foreach (var list in lists)
            {
                Assert.Equal(Enumerable.Range(0, 10).ToList(), list);
            }
        }
    }
}
