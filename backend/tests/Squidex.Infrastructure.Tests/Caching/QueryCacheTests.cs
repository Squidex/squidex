// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Infrastructure.Caching
{
    public class QueryCacheTests
    {
        private readonly IMemoryCache memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

        private record CachedEntry(int Value) : IWithId<int>
        {
            public int Id => Value;
        }

        [Fact]
        public async Task Should_query_from_cache()
        {
            var sut = new QueryCache<int, CachedEntry>();

            var (queried, result) = await ConfigureAsync(sut, 1, 2);

            Assert.Equal(new[] { 1, 2 }, queried);
            Assert.Equal(new[] { 1, 2 }, result);
        }

        [Fact]
        public async Task Should_query_pending_from_cache()
        {
            var sut = new QueryCache<int, CachedEntry>();

            var (queried1, result1) = await ConfigureAsync(sut, 1, 2);
            var (queried2, result2) = await ConfigureAsync(sut, 1, 2, 3, 4);

            Assert.Equal(new[] { 1, 2 }, queried1.ToArray());
            Assert.Equal(new[] { 3, 4 }, queried2.ToArray());

            Assert.Equal(new[] { 1, 2 }, result1.ToArray());
            Assert.Equal(new[] { 1, 2, 3, 4 }, result2.ToArray());
        }

        [Fact]
        public async Task Should_query_pending_from_cache_if_manually_added()
        {
            var sut = new QueryCache<int, CachedEntry>();

            sut.SetMany(new[] { (1, null), (2, new CachedEntry(2)) });

            var (queried, result) = await ConfigureAsync(sut, 1, 2, 3, 4);

            Assert.Equal(new[] { 3, 4 }, queried);
            Assert.Equal(new[] { 2, 3, 4 }, result);
        }

        [Fact]
        public async Task Should_query_pending_from_memory_cache_if_manually_added()
        {
            var sut1 = new QueryCache<int, CachedEntry>(memoryCache);
            var sut2 = new QueryCache<int, CachedEntry>(memoryCache);

            var cacheDuration = TimeSpan.FromSeconds(10);

            sut1.SetMany(new[] { (1, null), (2, new CachedEntry(2)) }, cacheDuration);

            var (queried, result) = await ConfigureAsync(sut2, x => true, cacheDuration, 1, 2, 3, 4);

            Assert.Equal(new[] { 3, 4 }, queried);
            Assert.Equal(new[] { 2, 3, 4 }, result);
        }

        [Fact]
        public async Task Should_query_pending_from_memory_cache_if_manually_added_but_not_added_permanently()
        {
            var sut1 = new QueryCache<int, CachedEntry>(memoryCache);
            var sut2 = new QueryCache<int, CachedEntry>(memoryCache);

            sut1.SetMany(new[] { (1, null), (2, new CachedEntry(2)) });

            var (queried, result) = await ConfigureAsync(sut2, 1, 2, 3, 4);

            Assert.Equal(new[] { 1, 2, 3, 4 }, queried);
            Assert.Equal(new[] { 1, 2, 3, 4 }, result);
        }

        [Fact]
        public async Task Should_query_pending_from_memory_cache_if_manually_added_but_not_queried_permanently()
        {
            var sut1 = new QueryCache<int, CachedEntry>(memoryCache);
            var sut2 = new QueryCache<int, CachedEntry>(memoryCache);

            var cacheDuration = TimeSpan.FromSeconds(10);

            sut1.SetMany(new[] { (1, null), (2, new CachedEntry(2)) }, cacheDuration);

            var (queried, result) = await ConfigureAsync(sut2, 1, 2, 3, 4);

            Assert.Equal(new[] { 1, 2, 3, 4 }, queried);
            Assert.Equal(new[] { 1, 2, 3, 4 }, result);
        }

        [Fact]
        public async Task Should_not_query_again_if_failed_before()
        {
            var sut = new QueryCache<int, CachedEntry>();

            var (queried1, result1) = await ConfigureAsync(sut, x => x > 1, default, 1, 2);
            var (queried2, result2) = await ConfigureAsync(sut, 1, 2, 3, 4);

            Assert.Equal(new[] { 1, 2 }, queried1.ToArray());
            Assert.Equal(new[] { 3, 4 }, queried2.ToArray());

            Assert.Equal(new[] { 2 }, result1.ToArray());
            Assert.Equal(new[] { 2, 3, 4 }, result2.ToArray());
        }

        [Fact]
        public async Task Should_query_from_memory_cache()
        {
            var sut1 = new QueryCache<int, CachedEntry>(memoryCache);
            var sut2 = new QueryCache<int, CachedEntry>(memoryCache);

            var cacheDuration = TimeSpan.FromSeconds(10);

            var (queried1, result1) = await ConfigureAsync(sut1, x => true, cacheDuration, 1, 2);
            var (queried2, result2) = await ConfigureAsync(sut2, x => true, cacheDuration, 1, 2, 3, 4);

            Assert.Equal(new[] { 1, 2 }, queried1.ToArray());
            Assert.Equal(new[] { 3, 4 }, queried2.ToArray());

            Assert.Equal(new[] { 1, 2 }, result1.ToArray());
            Assert.Equal(new[] { 1, 2, 3, 4 }, result2.ToArray());
        }

        [Fact]
        public async Task Should_not_query_from_memory_cache_if_not_queried_permanently()
        {
            var sut1 = new QueryCache<int, CachedEntry>(memoryCache);
            var sut2 = new QueryCache<int, CachedEntry>(memoryCache);

            var (queried1, result1) = await ConfigureAsync(sut1, x => true, null, 1, 2);
            var (queried2, result2) = await ConfigureAsync(sut2, x => true, null, 1, 2, 3, 4);

            Assert.Equal(new[] { 1, 2 }, queried1.ToArray());
            Assert.Equal(new[] { 1, 2, 3, 4 }, queried2.ToArray());

            Assert.Equal(new[] { 1, 2 }, result1.ToArray());
            Assert.Equal(new[] { 1, 2, 3, 4 }, result2.ToArray());
        }

        private static Task<(int[], int[])> ConfigureAsync(IQueryCache<int, CachedEntry> sut, params int[] ids)
        {
            return ConfigureAsync(sut, x => true, null, ids);
        }

        private static async Task<(int[], int[])> ConfigureAsync(IQueryCache<int, CachedEntry> sut, Func<int, bool> predicate, TimeSpan? cacheDuration, params int[] ids)
        {
            var queried = new HashSet<int>();

            var result = await sut.CacheOrQueryAsync(ids, async pending =>
            {
                queried.AddRange(pending);

                await Task.Yield();

                return pending.Where(predicate).Select(x => new CachedEntry(x));
            }, cacheDuration);

            return (queried.ToArray(), result.Select(x => x.Value).ToArray());
        }
    }
}
