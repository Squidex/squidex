// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Tasks;

public class PartitionedActionBlockTests
{
    private const int Partitions = 10;

    [Fact]
    public async Task Should_propagate_in_order()
    {
        var lists = new List<int>[Partitions];

        for (var i = 0; i < Partitions; i++)
        {
            lists[i] = [];
        }

        var scheduler = new PartitionedScheduler<(int Partition, int Value)>((item, ct) =>
        {
            Random.Shared.Next(10);

            lists[item.Partition].Add(item.Value);

            return Task.CompletedTask;
        }, 32, 10000);

        for (var partition = 0; partition < Partitions; partition++)
        {
            for (var value = 0; value < 10; value++)
            {
                await scheduler.ScheduleAsync(partition, (partition, value));
            }
        }

        await scheduler.CompleteAsync();

        foreach (var list in lists)
        {
            Assert.Equal(Enumerable.Range(0, 10).ToList(), list);
        }
    }
}
