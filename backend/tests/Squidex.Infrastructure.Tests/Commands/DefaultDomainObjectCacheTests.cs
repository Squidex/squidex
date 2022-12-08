// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure.Json;

namespace Squidex.Infrastructure.Commands;

public class DefaultDomainObjectCacheTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly IJsonSerializer serializer = A.Fake<IJsonSerializer>();
    private readonly IMemoryCache cache = A.Fake<IMemoryCache>();
    private readonly IDistributedCache distributedCache = A.Fake<IDistributedCache>();
    private readonly DomainId id = DomainId.NewGuid();
    private readonly DefaultDomainObjectCache sut;

    public DefaultDomainObjectCacheTests()
    {
        ct = cts.Token;

        var options = Options.Create(new DomainObjectCacheOptions());

        sut = new DefaultDomainObjectCache(cache, serializer, distributedCache, options);
    }

    [Fact]
    public async Task Should_add_to_cache_and_memory_cache_on_set()
    {
        await sut.SetAsync(id, 10, 20, ct);

        A.CallTo(() => cache.CreateEntry($"{id}_10"))
            .MustHaveHappened();

        A.CallTo(() => serializer.Serialize(20, A<Stream>._, true))
            .MustHaveHappened();

        A.CallTo(() => distributedCache.SetAsync($"{id}_10", A<byte[]>._, A<DistributedCacheEntryOptions>._, ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_ignore_exception_on_set()
    {
        A.CallTo(() => distributedCache.SetAsync(A<string>._, A<byte[]>._, A<DistributedCacheEntryOptions>._, ct))
            .Throws(new InvalidOperationException());

        await sut.SetAsync(id, 10, 20, ct);
    }

    [Fact]
    public async Task Should_provide_from_cache_if_found()
    {
        object? returned;

        A.CallTo(() => cache.TryGetValue($"{id}_10", out returned))
            .Returns(true)
            .AssignsOutAndRefParameters(20);

        var actual = await sut.GetAsync<int>(id, 10, ct);

        Assert.Equal(20, actual);
    }

    [Fact]
    public async Task Should_provide_from_distributed_cache_if_not_found_in_cache()
    {
        A.CallTo(() => serializer.Deserialize<int>(A<Stream>._, null, false))
            .Returns(20);

        var actual = await sut.GetAsync<int>(id, 10, ct);

        Assert.Equal(20, actual);

        A.CallTo(() => distributedCache.GetAsync($"{id}_10", ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_ignore_exception_on_gett()
    {
        A.CallTo(() => distributedCache.GetAsync(A<string>._, ct))
            .Throws(new InvalidOperationException());

        await sut.SetAsync(id, 10, 20, ct);
    }
}
