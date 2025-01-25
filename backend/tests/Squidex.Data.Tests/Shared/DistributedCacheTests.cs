// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Distributed;

namespace Squidex.Shared;

public abstract class DistributedCacheTests
{
    private readonly TimeProvider timeProvider = A.Fake<TimeProvider>();
    private DateTimeOffset now = DateTimeOffset.UtcNow;

    protected TimeProvider TimeProvider => timeProvider;

    protected DistributedCacheTests()
    {
        A.CallTo(() => timeProvider.GetUtcNow())
            .ReturnsLazily(() => now);
    }

    protected abstract Task<IDistributedCache> CreateSutAsync();

    [Fact]
    public async Task Should_add_and_get_entry_without_expiration()
    {
        var sut = await CreateSutAsync();

        var cacheKey = Guid.NewGuid().ToString();
        var cacheValue = cacheKey;

        var options = new DistributedCacheEntryOptions();

        await sut.SetStringAsync(cacheKey, cacheValue, options);

        var result = await sut.GetStringAsync(cacheKey);
        Assert.Equal(cacheKey, result);
    }

    [Fact]
    public async Task Should_add_and_get_entry()
    {
        var sut = await CreateSutAsync();

        var cacheKey = Guid.NewGuid().ToString();
        var cacheValue = cacheKey;

        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) };

        await sut.SetStringAsync(cacheKey, cacheValue, options);

        var result = await sut.GetStringAsync(cacheKey);
        Assert.Equal(cacheKey, result);
    }

    [Fact]
    public async Task Should_not_return_result_if_expired()
    {
        var sut = await CreateSutAsync();

        var cacheKey = Guid.NewGuid().ToString();
        var cacheValue = cacheKey;

        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) };

        await sut.SetStringAsync(cacheKey, cacheValue, options);

        now = now.AddDays(1);

        var result = await sut.GetStringAsync(cacheKey);
        Assert.Null(result);
    }

    [Fact]
    public async Task Should_not_return_result_if_removed()
    {
        var sut = await CreateSutAsync();

        var cacheKey = Guid.NewGuid().ToString();
        var cacheValue = cacheKey;

        var options = new DistributedCacheEntryOptions();

        await sut.SetStringAsync(cacheKey, cacheValue, options);
        await sut.RemoveAsync(cacheKey);

        var result = await sut.GetStringAsync(cacheKey);
        Assert.Null(result);
    }
}
