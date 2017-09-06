// ==========================================================================
//  InvalidatingMemoryCacheTest.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace Squidex.Infrastructure.Caching
{
    public class InvalidatingMemoryCacheTests
    {
        internal sealed class MyOptions<T> : IOptions<T> where T : class, new()
        {
            public MyOptions(T value)
            {
                Value = value;
            }

            public T Value { get; }
        }

        private readonly IPubSub pubsub = A.Fake<IPubSub>();
        private readonly IMemoryCache cache = A.Fake<IMemoryCache>();
        private readonly InvalidatingMemoryCache sut;

        public InvalidatingMemoryCacheTests()
        {
            sut = new InvalidatingMemoryCache(cache, pubsub);
        }

        [Fact]
        public void Should_do_nothing_if_cache_does_not_support_invalidation()
        {
            new MemoryCache(new MyOptions<MemoryCacheOptions>(new MemoryCacheOptions())).Invalidate("a-key");
        }

        [Fact]
        public void Should_dispose_inner_on_dispose()
        {
            sut.Dispose();

            A.CallTo(() => cache.Dispose()).MustHaveHappened();
        }

        [Fact]
        public void Should_call_inner_on_remove()
        {
            sut.Remove("a-key");

            A.CallTo(() => cache.Remove("a-key")).MustHaveHappened();
        }

        [Fact]
        public void Should_invalidate_if_key_is_not_a_string()
        {
            sut.Invalidate(123);

            A.CallTo(() => pubsub.Publish("CacheInvalidations", A<string>.Ignored, true)).MustNotHaveHappened();
        }

        [Fact]
        public void Should_invalidate_if_key_is_string()
        {
            sut.Invalidate("a-key");

            A.CallTo(() => pubsub.Publish("CacheInvalidations", "a-key", true)).MustHaveHappened();
        }

        [Fact]
        public void Should_invalidate_if_key_is_string_for_cache()
        {
            ((IMemoryCache)sut).Invalidate("a-key");

            A.CallTo(() => pubsub.Publish("CacheInvalidations", "a-key", true)).MustHaveHappened();
        }

        [Fact]
        public void Should_call_inner_to_create_value()
        {
            var cacheEntry = A.Dummy<ICacheEntry>();

            A.CallTo(() => cache.CreateEntry("a-key"))
                .Returns(cacheEntry);

            var result = sut.CreateEntry("a-key");

            Assert.Equal(cacheEntry, result);
        }

        [Fact]
        public void Should_use_inner_cache_to_get_value()
        {
            object outValue = 123;

            A.CallTo(() => cache.TryGetValue("a-key", out outValue))
                .Returns(true);

            var exists = sut.TryGetValue("a-key", out var result);

            Assert.Equal(123, result);
            Assert.True(exists);
        }

        [Fact]
        public void Should_remove_if_invalidated()
        {
            var anotherPubsub = new InMemoryPubSub();
            var anotherSut = new InvalidatingMemoryCache(new MemoryCache(new MyOptions<MemoryCacheOptions>(new MemoryCacheOptions())), anotherPubsub);

            anotherSut.Set("a-key", 123);

            Assert.Equal(123, anotherSut.Get<int>("a-key"));

            anotherPubsub.Publish("CacheInvalidations", "a-key", true);

            Assert.Equal(0, anotherSut.Get<int>("a-key"));
        }
    }
}
