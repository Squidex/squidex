// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Xunit;

namespace Squidex.Infrastructure.Caching
{
    public class AsyncLocalCacheTests
    {
        private readonly ILocalCache sut = new AsyncLocalCache();
        private int called;

        [Fact]
        public async Task Should_add_item_to_cache_when_context_exists()
        {
            using (sut.StartContext())
            {
                sut.Add("Key", 1);

                await Task.Delay(5);

                AssertCache(sut, "Key", 1, true);

                await Task.Delay(5);

                sut.Remove("Key");

                AssertCache(sut, "Key", null, false);
            }
        }

        [Fact]
        public async Task Should_not_add_item_to_cache_when_context_not_exists()
        {
            sut.Add("Key", 1);

            await Task.Delay(5);

            AssertCache(sut, "Key", null, false);

            sut.Remove("Key");

            await Task.Delay(5);

            AssertCache(sut, "Key", null, false);
        }

        [Fact]
        public async Task Should_call_creator_once_when_context_exists()
        {
            using (sut.StartContext())
            {
                var value1 = await sut.GetOrCreateAsync("Key", () => Task.FromResult(++called));

                await Task.Delay(5);

                var value2 = await sut.GetOrCreateAsync("Key", () => Task.FromResult(++called));

                Assert.Equal(1, called);
                Assert.Equal(1, value1);
                Assert.Equal(1, value2);
            }
        }

        [Fact]
        public async Task Should_call_creator_twice_when_context_not_exists()
        {
            var value1 = await sut.GetOrCreateAsync("Key", () => Task.FromResult(++called));

            await Task.Delay(5);

            var value2 = await sut.GetOrCreateAsync("Key", () => Task.FromResult(++called));

            Assert.Equal(2, called);
            Assert.Equal(1, value1);
            Assert.Equal(2, value2);
        }

        [Fact]
        public async Task Should_call_async_creator_once_when_context_exists()
        {
            using (sut.StartContext())
            {
                var value1 = await sut.GetOrCreateAsync("Key", () => Task.FromResult(++called));

                await Task.Delay(5);

                var value2 = await sut.GetOrCreateAsync("Key", () => Task.FromResult(++called));

                Assert.Equal(1, called);
                Assert.Equal(1, value1);
                Assert.Equal(1, value2);
            }
        }

        [Fact]
        public async Task Should_call_async_creator_twice_when_context_not_exists()
        {
            var value1 = await sut.GetOrCreateAsync("Key", () => Task.FromResult(++called));

            await Task.Delay(5);

            var value2 = await sut.GetOrCreateAsync("Key", () => Task.FromResult(++called));

            Assert.Equal(2, called);
            Assert.Equal(1, value1);
            Assert.Equal(2, value2);
        }

        private static void AssertCache(ILocalCache cache, string key, object? expectedValue, bool expectedFound)
        {
            var found = cache.TryGetValue(key, out var value);

            Assert.Equal(expectedFound, found);
            Assert.Equal(expectedValue, value);
        }
    }
}
