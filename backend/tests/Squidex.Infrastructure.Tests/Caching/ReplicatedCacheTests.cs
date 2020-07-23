// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace Squidex.Infrastructure.Caching
{
    public class ReplicatedCacheTests
    {
        private readonly IPubSub pubSub = A.Fake<SimplePubSub>(options => options.CallsBaseMethods());
        private readonly ReplicatedCacheOptions options = new ReplicatedCacheOptions { Enable = true };
        private readonly ReplicatedCache sut;

        public ReplicatedCacheTests()
        {
            sut = new ReplicatedCache(CreateMemoryCache(), pubSub, Options.Create(options));
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
        public void Should_not_serve_from_cache_disabled()
        {
            options.Enable = false;

            sut.Add("Key", 1, TimeSpan.FromMilliseconds(100), true);

            AssertCache(sut, "Key", null, false);
        }

        [Fact]
        public async Task Should_not_serve_from_cache_when_expired()
        {
            sut.Add("Key", 1, TimeSpan.FromMilliseconds(1), true);

            await Task.Delay(100);

            AssertCache(sut, "Key", null, false);
        }

        [Fact]
        public void Should_not_invalidate_other_instances_when_item_added_and_flag_is_false()
        {
            var cache1 = new ReplicatedCache(CreateMemoryCache(), pubSub, Options.Create(options));
            var cache2 = new ReplicatedCache(CreateMemoryCache(), pubSub, Options.Create(options));

            cache1.Add("Key", 1, TimeSpan.FromMinutes(1), false);
            cache2.Add("Key", 2, TimeSpan.FromMinutes(1), false);

            AssertCache(cache1, "Key", 1, true);
            AssertCache(cache2, "Key", 2, true);
        }

        [Fact]
        public void Should_invalidate_other_instances_when_item_added_and_flag_is_true()
        {
            var cache1 = new ReplicatedCache(CreateMemoryCache(), pubSub, Options.Create(options));
            var cache2 = new ReplicatedCache(CreateMemoryCache(), pubSub, Options.Create(options));

            cache1.Add("Key", 1, TimeSpan.FromMinutes(1), true);
            cache2.Add("Key", 2, TimeSpan.FromMinutes(1), true);

            AssertCache(cache1, "Key", null, false);
            AssertCache(cache2, "Key", 2, true);
        }

        [Fact]
        public void Should_invalidate_other_instances_when_item_removed()
        {
            var cache1 = new ReplicatedCache(CreateMemoryCache(), pubSub, Options.Create(options));
            var cache2 = new ReplicatedCache(CreateMemoryCache(), pubSub, Options.Create(options));

            cache1.Add("Key", 1, TimeSpan.FromMinutes(1), true);
            cache2.Remove("Key");

            AssertCache(cache1, "Key", null, false);
            AssertCache(cache2, "Key", null, false);
        }

        [Fact]
        public void Should_send_invalidation_message_when_added_and_flag_is_true()
        {
            sut.Add("Key", 1, TimeSpan.FromMinutes(1), true);

            A.CallTo(() => pubSub.Publish(A<object>._))
                .MustHaveHappened();
        }

        [Fact]
        public void Should_not_send_invalidation_message_when_added_flag_is_false()
        {
            sut.Add("Key", 1, TimeSpan.FromMinutes(1), false);

            A.CallTo(() => pubSub.Publish(A<object>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public void Should_not_send_invalidation_message_when_added_but_disabled()
        {
            options.Enable = false;

            sut.Add("Key", 1, TimeSpan.FromMinutes(1), true);

            A.CallTo(() => pubSub.Publish(A<object>._))
                .MustNotHaveHappened();
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
