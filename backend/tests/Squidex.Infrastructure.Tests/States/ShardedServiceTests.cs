// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Hosting;

namespace Squidex.Infrastructure.States;

public class ShardedServiceTests
{
    private readonly IInner inner1 = A.Fake<IInner>();
    private readonly IInner inner2 = A.Fake<IInner>();
    private readonly TestSut sut;

    public interface IInner : IInitializable
    {
    }

    private class TestSut : ShardedService<DomainId, IInner>
    {
        public TestSut(IShardingStrategy sharding, Func<string, IInner> factory)
            : base(sharding, factory)
        {
        }

        public IInner ExposeShard(DomainId key)
        {
            return Shard(key);
        }
    }

    public ShardedServiceTests()
    {
        sut = new TestSut(new PartitionedSharding(2), key =>
        {
            if (key == "_0")
            {
                return inner1;
            }
            else
            {
                return inner2;
            }
        });

        sut.InitializeAsync(default).Wait();
    }

    [Fact]
    public async Task Should_initialize_shards()
    {
        await sut.InitializeAsync(default);

        A.CallTo(() => inner1.InitializeAsync(A<CancellationToken>._))
            .MustHaveHappened();

        A.CallTo(() => inner2.InitializeAsync(A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_release_shards()
    {
        await sut.ReleaseAsync(default);

        A.CallTo(() => inner1.ReleaseAsync(A<CancellationToken>._))
            .MustHaveHappened();

        A.CallTo(() => inner2.ReleaseAsync(A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public void Should_provide_shards()
    {
        Assert.Equal(inner1, sut.ExposeShard(DomainId.Create("2")));
        Assert.Equal(inner2, sut.ExposeShard(DomainId.Create("3")));
    }
}
