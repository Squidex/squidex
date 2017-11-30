// ==========================================================================
//  StatesTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
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
        private readonly IPubSub pubSub = new InMemoryPubSub(true);
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
        public async Task Should_read_from_store()
        {
            A.CallTo(() => store.ReadAsync<int>(key))
                .Returns((123, Guid.NewGuid().ToString()));

            var actual = await sut.GetAsync<MyStatefulObject, int>(key);

            Assert.Same(state, actual);
            Assert.NotNull(cache.Get<object>(key));

            Assert.Equal(123, state.State);
        }

        [Fact]
        public async Task Should_provide_state_from_services_and_add_to_cache()
        {
            var actual = await sut.GetAsync<MyStatefulObject, int>(key);

            Assert.Same(state, actual);
            Assert.NotNull(cache.Get<object>(key));
        }

        [Fact]
        public async Task Should_serve_next_request_from_cache()
        {
            var actual1 = await sut.GetAsync<MyStatefulObject, int>(key);

            Assert.Same(state, actual1);
            Assert.NotNull(cache.Get<object>(key));

            var actual2 = await sut.GetAsync<MyStatefulObject, int>(key);

            Assert.Same(state, actual2);

            A.CallTo(() => services.GetService(typeof(MyStatefulObject)))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task Should_write_to_store_with_previous_etag()
        {
            var etag = Guid.NewGuid().ToString();

            InvalidateMessage message = null;

            pubSub.Subscribe<InvalidateMessage>(m =>
            {
                message = m;
            });

            A.CallTo(() => store.ReadAsync<int>(key))
                .Returns((123, etag));

            var actual = await sut.GetAsync<MyStatefulObject, int>(key);

            Assert.Same(state, actual);
            Assert.Equal(123, state.State);

            state.SetState(456);

            await state.WriteStateAsync();

            A.CallTo(() => store.WriteAsync(key, 456, etag, A<string>.That.Matches(x => x != null)))
                .MustHaveHappened();

            Assert.NotNull(message);
            Assert.Equal(key, message.Key);
        }

        [Fact]
        public async Task Should_remove_from_cache_when_invalidation_message_received()
        {
            var actual = await sut.GetAsync<MyStatefulObject, int>(key);

            await InvalidateCacheAsync();

            Assert.False(cache.TryGetValue(key, out var t));
        }

        [Fact]
        public async Task Should_return_same_instance_for_parallel_requests()
        {
            A.CallTo(() => store.ReadAsync<int>(key))
                .ReturnsLazily(() => Task.Delay(1).ContinueWith(x => (1, "1")));

            var tasks = new List<Task<MyStatefulObject>>();

            for (var i = 0; i < 1000; i++)
            {
                tasks.Add(Task.Run(() => sut.GetAsync<MyStatefulObject, int>(key)));
            }

            var retrievedStates = await Task.WhenAll(tasks);

            foreach (var retrievedState in retrievedStates)
            {
                Assert.Same(retrievedStates[0], retrievedState);
            }

            A.CallTo(() => store.ReadAsync<int>(key))
                .MustHaveHappened(Repeated.Exactly.Once);
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
