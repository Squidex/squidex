// ==========================================================================
//  InvalidatingMemoryCacheTest.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

// ReSharper disable RedundantAssignment

namespace Squidex.Infrastructure.Caching
{
    public class InvalidatingMemoryCacheTest
    {
        internal sealed class MyOptions<T> : IOptions<T> where T : class, new()
        {
            public MyOptions(T value)
            {
                Value = value;
            }

            public T Value { get; }
        }

        private readonly Mock<IPubSub> pubsub = new Mock<IPubSub>();
        private readonly Mock<IMemoryCache> cache = new Mock<IMemoryCache>();
        private readonly InvalidatingMemoryCache sut;

        public InvalidatingMemoryCacheTest()
        {
            sut = new InvalidatingMemoryCache(cache.Object, pubsub.Object);
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

            cache.Verify(x => x.Dispose(), Times.Once());
        }

        [Fact]
        public void Should_call_inner_on_remove()
        {
            sut.Remove("a-key");

            cache.Verify(x => x.Remove("a-key"), Times.Once());
        }

        [Fact]
        public void Should_invalidate_if_key_is_not_a_string()
        {
            sut.Invalidate(123);

            pubsub.Verify(x => x.Publish("CacheInvalidations", It.IsAny<string>(), true), Times.Never());
        }

        [Fact]
        public void Should_invalidate_if_key_is_string()
        {
            sut.Invalidate("a-key");

            pubsub.Verify(x => x.Publish("CacheInvalidations", "a-key", true), Times.Once());
        }

        [Fact]
        public void Should_invalidate_if_key_is_string_for_cache()
        {
            ((IMemoryCache)sut).Invalidate("a-key");

            pubsub.Verify(x => x.Publish("CacheInvalidations", "a-key", true), Times.Once());
        }

        [Fact]
        public void Should_call_inner_to_create_value()
        {
            var cacheEntry = new Mock<ICacheEntry>();

            cache.Setup(x => x.CreateEntry("a-key")).Returns(cacheEntry.Object);

            var result = sut.CreateEntry("a-key");

            Assert.Equal(cacheEntry.Object, result);
        }

        [Fact]
        public void Should_use_inner_cache_to_get_value()
        {
            object currentOut = 123;

            cache.Setup(x => x.TryGetValue("a-key", out currentOut)).Returns(true);

            object result;

            var exists = sut.TryGetValue("a-key", out result);

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
