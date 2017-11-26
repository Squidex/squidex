// ==========================================================================
//  StatesTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace Squidex.Infrastructure.States
{
    public class StatesTests
    {
        private class MyStatefulObject : StatefulObject<int>
        {
            public void SetState(int value)
            {
                State = value;
            }
        }

        private readonly string key = Guid.NewGuid().ToString();
        private readonly MyStatefulObject state = new MyStatefulObject();
        private readonly IMemoryCache cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        private readonly IPubSub pubSub = new InMemoryPubSub();
        private readonly IServiceProvider services = A.Fake<IServiceProvider>();
        private readonly IStateStore store = A.Fake<IStateStore>();
        private readonly StateFactory sut;

        public StatesTests()
        {
            A.CallTo(() => services.GetService(typeof(MyStatefulObject)))
                .Returns(state);

            sut = new StateFactory(pubSub, services, store, cache);
            sut.Connect();
        }

        [Fact]
        public async Task Should_provide_object_from_cache()
        {
            cache.Set(key, state);

            var actual = await sut.GetAsync<MyStatefulObject, int>(key);

            Assert.Same(state, actual);
        }

        [Fact]
        public async Task Should_read_from_store()
        {
            A.CallTo(() => store.ReadAsync<int>(key))
                .Returns((123, Guid.NewGuid().ToString()));

            var actual = await sut.GetAsync<MyStatefulObject, int>(key);

            Assert.Same(state, actual);
            Assert.Same(state, cache.Get<MyStatefulObject>(key));

            Assert.Equal(123, state.State);
        }

        [Fact]
        public async Task Should_provide_state_from_services_and_add_to_cache()
        {
            var actual = await sut.GetAsync<MyStatefulObject, int>(key);

            Assert.Same(state, actual);
            Assert.Same(state, cache.Get<MyStatefulObject>(key));
        }

        [Fact]
        public async Task Should_serve_next_request_from_cache()
        {
            var actual1 = await sut.GetAsync<MyStatefulObject, int>(key);

            Assert.Same(state, actual1);
            Assert.Same(state, cache.Get<MyStatefulObject>(key));

            var actual2 = await sut.GetAsync<MyStatefulObject, int>(key);

            Assert.Same(state, actual2);
            Assert.Same(state, cache.Get<MyStatefulObject>(key));

            A.CallTo(() => services.GetService(typeof(MyStatefulObject)))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task Should_write_to_store_with_previous_etag()
        {
            var etag = Guid.NewGuid().ToString();

            A.CallTo(() => store.ReadAsync<int>(key))
                .Returns((123, etag));

            var actual = await sut.GetAsync<MyStatefulObject, int>(key);

            Assert.Same(state, actual);
            Assert.Same(state, cache.Get<MyStatefulObject>(key));

            Assert.Equal(123, state.State);

            state.SetState(456);

            await state.WriteStateAsync();

            A.CallTo(() => store.WriteAsync(key, 456, etag, A<string>.That.Matches(x => x != null)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_remove_from_cache_when_message_sent()
        {
            var actual = await sut.GetAsync<MyStatefulObject, int>(key);

            await InvalidateCacheAsync();

            Assert.True(actual.IsDisposed);
        }

        [Fact]
        public async Task Should_not_dispose_detached_when_message_sent()
        {
            var actual = await sut.GetDetachedAsync<MyStatefulObject, int>(key);

            await InvalidateCacheAsync();

            Assert.False(actual.IsDisposed);
        }

        [Fact]
        public async Task Should_dispose_states_if_exired()
        {
            var actual = await sut.GetAsync<MyStatefulObject, int>(key);

            await RemoveFromCacheAsync();

            Assert.True(actual.IsDisposed);
        }

        [Fact]
        public async Task Should_not_dispose_detached_states_if_exired()
        {
            var actual = await sut.GetDetachedAsync<MyStatefulObject, int>(key);

            await RemoveFromCacheAsync();

            Assert.False(actual.IsDisposed);
        }

        [Fact]
        public async Task Should_dispose_states_if_disposed()
        {
            var actual = await sut.GetAsync<MyStatefulObject, int>(key);

            sut.Dispose();

            Assert.True(actual.IsDisposed);
        }

        [Fact]
        public async Task Should_not_dispose_detached_states_if_disposed()
        {
            var actual = await sut.GetDetachedAsync<MyStatefulObject, int>(key);

            sut.Dispose();

            Assert.False(actual.IsDisposed);
        }

        private async Task RemoveFromCacheAsync()
        {
            cache.Remove(key);

            await Task.Delay(400);
        }

        private async Task InvalidateCacheAsync()
        {
            pubSub.Publish(new InvalidateMessage { Key = key }, true);

            await Task.Delay(400);
        }
    }
}
