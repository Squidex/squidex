// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Squidex.Infrastructure.Caching;

public class QueryCacheTests
{
    private readonly IMemoryCache memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

    [Fact]
    public void Should_query_from_cache()
    {
        var sut = new QueryCache<int, int>(memoryCache);

        sut.Set(1, 1, TimeSpan.FromHours(1));

        var result1 = sut.TryGet(1, out var found1);
        var result2 = sut.TryGet(2, out var found2);

        Assert.True(result1);
        Assert.Equal(1, found1);

        Assert.False(result2);
        Assert.Equal(0, found2);
    }

    [Fact]
    public void Should_not_query_from_cache_if_not_configured()
    {
        var sut = new QueryCache<int, int>();

        sut.Set(1, 1, TimeSpan.FromHours(1));

        var result1 = sut.TryGet(1, out var found1);
        var result2 = sut.TryGet(2, out var found2);

        Assert.False(result1);
        Assert.Equal(0, found1);

        Assert.False(result2);
        Assert.Equal(0, found2);
    }
}
