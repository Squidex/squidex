// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.States;

public class ShardingTests
{
    [Fact]
    public void Should_provide_single_shard_key()
    {
        var strategy = SingleSharding.Instance;

        for (var i = 0; i < 1000; i++)
        {
            var shardKey = strategy.GetShardKey(DomainId.NewGuid());

            Assert.Equal(string.Empty, shardKey);
        }
    }

    [Fact]
    public void Should_provide_single_shard_keys()
    {
        var strategy = SingleSharding.Instance;

        var shardKeys = strategy.GetShardKeys().ToArray();

        Assert.Equal(new[] { string.Empty }, shardKeys);
    }

    [Fact]
    public void Should_provide_partitioned_shard_key()
    {
        var strategy = new PartitionedSharding(3);

        for (var i = 0; i < 1000; i++)
        {
            var shardKey = strategy.GetShardKey(DomainId.NewGuid());

            Assert.True(shardKey is "_0" or "_1" or "_2");
        }
    }

    [Fact]
    public void Should_provide_partitioned_shard_keys()
    {
        var strategy = new PartitionedSharding(3);

        var shardKeys = strategy.GetShardKeys().ToArray();

        Assert.Equal(new[] { "_0", "_1", "_2" }, shardKeys);
    }
}
