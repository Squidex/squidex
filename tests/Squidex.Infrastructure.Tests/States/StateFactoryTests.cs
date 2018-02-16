// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure.Tasks;
using Xunit;

#pragma warning disable RECS0002 // Convert anonymous method to method group

namespace Squidex.Infrastructure.States
{
    public class StateFactoryTests : IDisposable
    {
        private class MyStatefulObject : IStatefulObject<string>
        {
            public Task ActivateAsync(string key)
            {
                return TaskHelper.Done;
            }
        }

        private readonly string key = Guid.NewGuid().ToString();
        private readonly MyStatefulObject statefulObject = new MyStatefulObject();
        private readonly IMemoryCache cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        private readonly IPubSub pubSub = new InMemoryPubSub(true);
        private readonly IServiceProvider services = A.Fake<IServiceProvider>();
        private readonly StateFactory sut;

        public StateFactoryTests()
        {
            A.CallTo(() => services.GetService(typeof(MyStatefulObject)))
                .Returns(statefulObject);

            sut = new StateFactory(pubSub, cache, services);
            sut.Initialize();
        }

        public void Dispose()
        {
            sut.Dispose();
        }

        [Fact]
        public async Task Should_provide_state_from_services_and_add_to_cache()
        {
            var actualObject = await sut.GetSingleAsync<MyStatefulObject, string>(key);

            Assert.Same(statefulObject, actualObject);
            Assert.NotNull(cache.Get<object>(key));
        }

        [Fact]
        public async Task Should_serve_next_request_from_cache()
        {
            var actualObject1 = await sut.GetSingleAsync<MyStatefulObject, string>(key);

            Assert.Same(statefulObject, actualObject1);
            Assert.NotNull(cache.Get<object>(key));

            var actualObject2 = await sut.GetSingleAsync<MyStatefulObject, string>(key);

            A.CallTo(() => services.GetService(typeof(MyStatefulObject)))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Fact]
        public async Task Should_not_serve_next_request_from_cache_when_detached()
        {
            var actualObject1 = await sut.CreateAsync<MyStatefulObject, string>(key);

            Assert.Same(statefulObject, actualObject1);
            Assert.Null(cache.Get<object>(key));

            var actualObject2 = await sut.CreateAsync<MyStatefulObject, string>(key);

            A.CallTo(() => services.GetService(typeof(MyStatefulObject)))
                .MustHaveHappened(Repeated.Exactly.Twice);
        }

        [Fact]
        public async Task Should_remove_from_cache_when_invalidation_message_received()
        {
            var actualObject = await sut.GetSingleAsync<MyStatefulObject, string>(key);

            await InvalidateCacheAsync();

            Assert.False(cache.TryGetValue(key, out var t));
        }

        [Fact]
        public async Task Should_remove_from_cache_when_method_called()
        {
            var actualObject = await sut.GetSingleAsync<MyStatefulObject>(key);

            sut.Remove<MyStatefulObject, string>(key);

            Assert.False(cache.TryGetValue(key, out var t));
        }

        [Fact]
        public void Should_send_invalidation_message_on_refresh()
        {
            InvalidateMessage message = null;

            pubSub.Subscribe<InvalidateMessage>(m =>
            {
                message = m;
            });

            sut.Synchronize<MyStatefulObject, string>(key);

            Assert.NotNull(message);
            Assert.Equal(key, message.Key);
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