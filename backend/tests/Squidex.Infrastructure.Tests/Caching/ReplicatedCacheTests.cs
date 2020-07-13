// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace Squidex.Infrastructure.Caching
{
    public class ReplicatedCacheTests
    {
        private readonly IPubSub pubSub = new SimplePubSub();
        private readonly ReplicatedCache sut;

        public ReplicatedCacheTests()
        {
            sut = new ReplicatedCache(CreateMemoryCache(), pubSub);
        }

        [Fact]
        public void Should_serve_from_cache()
        {
            sut.Add("Key", 1, TimeSpan.FromMinutes(10), true);

            AssertCache(sut, "Key", 1, true);

            sut.Remove("Key");

            AssertCache(sut, "Key", null, false);
        }

        [Fact]
        public async Task Should_not_served_when_expired()
        {
            sut.Add("Key", 1, TimeSpan.FromMilliseconds(1), true);

            await Task.Delay(100);

            AssertCache(sut, "Key", null, false);
        }

        [Fact]
        public void Should_not_invalidate_other_instances_when_item_added_and_flag_is_false()
        {
            var cache1 = new ReplicatedCache(CreateMemoryCache(), pubSub);
            var cache2 = new ReplicatedCache(CreateMemoryCache(), pubSub);

            cache1.Add("Key", 1, TimeSpan.FromMinutes(1), false);
            cache2.Add("Key", 2, TimeSpan.FromMinutes(1), false);

            AssertCache(cache1, "Key", 1, true);
            AssertCache(cache2, "Key", 2, true);
        }

        [Fact]
        public void Should_invalidate_other_instances_when_item_added_and_flag_is_true()
        {
            var cache1 = new ReplicatedCache(CreateMemoryCache(), pubSub);
            var cache2 = new ReplicatedCache(CreateMemoryCache(), pubSub);

            cache1.Add("Key", 1, TimeSpan.FromMinutes(1), true);
            cache2.Add("Key", 2, TimeSpan.FromMinutes(1), true);

            AssertCache(cache1, "Key", null, false);
            AssertCache(cache2, "Key", 2, true);
        }

        [Fact]
        public void Should_invalidate_other_instances_when_item_removed()
        {
            var cache1 = new ReplicatedCache(CreateMemoryCache(), pubSub);
            var cache2 = new ReplicatedCache(CreateMemoryCache(), pubSub);

            cache1.Add("Key", 1, TimeSpan.FromMinutes(1), true);
            cache2.Remove("Key");

            AssertCache(cache1, "Key", null, false);
            AssertCache(cache2, "Key", null, false);
        }

        private static void AssertCache(IReplicatedCache cache, string key, object? expectedValue, bool expectedFound)
        {
            var found = cache.TryGetValue(key, out var value);

            Assert.Equal(expectedFound, found);
            Assert.Equal(expectedValue, value);
        }

        private static MemoryCache CreateMemoryCache()
        {
            return new MemoryCache(Options.Create(new MemoryCacheOptions()));
        }
    }
}
