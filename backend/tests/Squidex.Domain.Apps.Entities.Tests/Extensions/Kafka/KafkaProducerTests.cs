// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#if INCLUDE_KAFKA
using Squidex.Extensions.Actions.Kafka;

namespace Squidex.Domain.Apps.Entities.Extensions.Kafka;

public class KafkaProducerTests
{
    [Theory]
    [InlineData("key1", 12, 5)]
    [InlineData("key2", 12, 5)]
    [InlineData("key3", 12, 1)]
    public void Should_calculate_stable_partition(string partitionKey, int partitionCount, int expected)
    {
        var partition = KafkaProducer.GetPartition(partitionKey, partitionCount);

        Assert.Equal(expected, partition);
    }

    [Fact]
    public void Should_calculate_partition_in_range()
    {
        for (var i = 0; i < 1000; i++)
        {
            var partition = KafkaProducer.GetPartition($"key{i}", 7);

            Assert.InRange(partition, 0, 6);
        }
    }
}
#endif
