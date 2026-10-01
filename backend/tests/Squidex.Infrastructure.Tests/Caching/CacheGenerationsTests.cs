// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Squidex.Infrastructure.Caching;

public class CacheGenerationsTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly IDistributedCache cache = A.Fake<IDistributedCache>(x => x.Wrapping(new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()))));
    private readonly CacheGenerations sut;

    public CacheGenerationsTests()
    {
        ct = cts.Token;

        var options = Options.Create(new CacheGenerationsOptions { ResetInterval = TimeSpan.FromMilliseconds(100) });

        sut = new CacheGenerations(cache, options, A.Fake<ILogger<CacheGenerations>>());
    }

    [Fact]
    public async Task Should_create_generation_if_not_found()
    {
        var generation1 = await sut.GetAsync("key", ct);
        var generation2 = await sut.GetAsync("key", ct);

        Assert.NotEmpty(generation1);
        Assert.Equal(generation1, generation2);
    }

    [Fact]
    public async Task Should_use_separate_generations_for_different_keys()
    {
        var generation1 = await sut.GetAsync("key1", ct);
        var generation2 = await sut.GetAsync("key2", ct);

        Assert.NotEqual(generation1, generation2);
    }

    [Fact]
    public async Task Should_store_generation_without_expiration()
    {
        await sut.GetAsync("key", ct);

        A.CallTo(() => cache.SetAsync("generation/key", A<byte[]>._,
                A<DistributedCacheEntryOptions>.That.Matches(x =>
                    x.AbsoluteExpiration == null &&
                    x.AbsoluteExpirationRelativeToNow == null &&
                    x.SlidingExpiration == null),
                A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_not_change_generation_on_reset_before_flush()
    {
        var generation = await sut.GetAsync("key", ct);

        sut.Reset("key");

        Assert.Equal(generation, await sut.GetAsync("key", ct));
    }

    [Fact]
    public async Task Should_change_generation_after_reset_and_flush()
    {
        var generation = await sut.GetAsync("key", ct);

        sut.Reset("key");
        await sut.FlushAsync(ct);

        Assert.NotEqual(generation, await sut.GetAsync("key", ct));
    }

    [Fact]
    public async Task Should_write_each_reset_key_once_on_flush()
    {
        sut.Reset("key1");
        sut.Reset("key1");
        sut.Reset("key2");

        await sut.FlushAsync(ct);
        await sut.FlushAsync(ct);

        A.CallTo(() => cache.SetAsync("generation/key1", A<byte[]>._, A<DistributedCacheEntryOptions>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => cache.SetAsync("generation/key2", A<byte[]>._, A<DistributedCacheEntryOptions>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_not_throw_if_flush_fails()
    {
        A.CallTo(() => cache.SetAsync(A<string>._, A<byte[]>._, A<DistributedCacheEntryOptions>._, A<CancellationToken>._))
            .Throws(new InvalidOperationException());

        sut.Reset("key");
        await sut.FlushAsync(ct);
    }

    [Fact]
    public async Task Should_flush_resets_with_timer()
    {
        await sut.StartAsync(ct);
        try
        {
            sut.Reset("key");

            await Task.Delay(500, ct);

            A.CallTo(() => cache.SetAsync("generation/key", A<byte[]>._, A<DistributedCacheEntryOptions>._, A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }
        finally
        {
            await sut.StopAsync(ct);
        }
    }

    [Fact]
    public async Task Should_flush_pending_resets_when_stopped()
    {
        await sut.StartAsync(ct);

        sut.Reset("key");
        await sut.StopAsync(ct);

        A.CallTo(() => cache.SetAsync("generation/key", A<byte[]>._, A<DistributedCacheEntryOptions>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}
