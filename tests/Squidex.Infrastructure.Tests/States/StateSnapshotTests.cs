// ==========================================================================
//  StateSnapshotTests.cs
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
using Squidex.Infrastructure.EventSourcing;
using Xunit;

#pragma warning disable RECS0002 // Convert anonymous method to method group

namespace Squidex.Infrastructure.States
{
    public class StateSnapshotTests : IDisposable
    {
        private class MyStatefulObject : IStatefulObject<string>
        {
            private IPersistence<int> persistence;
            private int state;

            public long ExpectedVersion { get; set; }

            public long Version
            {
                get { return persistence.Version; }
            }

            public int State
            {
                get { return state; }
            }

            public Task ActivateAsync(string key, IStore<string> store)
            {
                persistence = store.WithSnapshots<int, string>(key, s => state = s);

                return persistence.ReadAsync(ExpectedVersion);
            }

            public void SetState(int value)
            {
                state = value;
            }

            public Task WriteStateAsync()
            {
                return persistence.WriteSnapshotAsync(state);
            }
        }

        private readonly string key = Guid.NewGuid().ToString();
        private readonly MyStatefulObject statefulObject = new MyStatefulObject();
        private readonly IEventDataFormatter eventDataFormatter = A.Fake<IEventDataFormatter>();
        private readonly IEventStore eventStore = A.Fake<IEventStore>();
        private readonly IMemoryCache cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        private readonly IPubSub pubSub = new InMemoryPubSub(true);
        private readonly IServiceProvider services = A.Fake<IServiceProvider>();
        private readonly ISnapshotStore<int, string> snapshotStore = A.Fake<ISnapshotStore<int, string>>();
        private readonly IStreamNameResolver streamNameResolver = A.Fake<IStreamNameResolver>();
        private readonly StateFactory sut;

        public StateSnapshotTests()
        {
            A.CallTo(() => services.GetService(typeof(MyStatefulObject)))
                .Returns(statefulObject);
            A.CallTo(() => services.GetService(typeof(ISnapshotStore<int, string>)))
                .Returns(snapshotStore);

            sut = new StateFactory(pubSub, cache, eventStore, eventDataFormatter, services, streamNameResolver);
            sut.Connect();
        }

        public void Dispose()
        {
            sut.Dispose();
        }

        [Fact]
        public async Task Should_read_from_store()
        {
            statefulObject.ExpectedVersion = 1;

            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((123, 1));

            var actualObject = await sut.GetSingleAsync<MyStatefulObject, string>(key);

            Assert.Same(statefulObject, actualObject);
            Assert.NotNull(cache.Get<object>(key));

            Assert.Equal(123, statefulObject.State);
        }

        [Fact]
        public async Task Should_set_to_empty_when_store_returns_not_found()
        {
            statefulObject.ExpectedVersion = EtagVersion.Any;

            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((123, EtagVersion.NotFound));

            var actualObject = await sut.GetSingleAsync<MyStatefulObject, string>(key);

            Assert.Equal(-1, statefulObject.Version);
            Assert.Equal( 0, statefulObject.State);
        }

        [Fact]
        public async Task Should_throw_exception_if_not_found()
        {
            statefulObject.ExpectedVersion = 0;

            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((0, EtagVersion.Empty));

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.GetSingleAsync<MyStatefulObject, string>(key));
        }

        [Fact]
        public async Task Should_throw_exception_if_other_version_found()
        {
            statefulObject.ExpectedVersion = 1;

            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((2, 2));

            await Assert.ThrowsAsync<DomainObjectVersionException>(() => sut.GetSingleAsync<MyStatefulObject, string>(key));
        }

        [Fact]
        public async Task Should_not_throw_exception_if_noting_expected()
        {
            statefulObject.ExpectedVersion = EtagVersion.Any;

            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((0, EtagVersion.Empty));

            await sut.GetSingleAsync<MyStatefulObject, string>(key);
        }

        [Fact]
        public async Task Should_provide_state_from_services_and_add_to_cache()
        {
            statefulObject.ExpectedVersion = EtagVersion.Any;

            var actualObject = await sut.GetSingleAsync<MyStatefulObject, string>(key);

            Assert.Same(statefulObject, actualObject);
            Assert.NotNull(cache.Get<object>(key));
        }

        [Fact]
        public async Task Should_serve_next_request_from_cache()
        {
            statefulObject.ExpectedVersion = EtagVersion.Any;

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
            statefulObject.ExpectedVersion = EtagVersion.Any;

            var actualObject1 = await sut.CreateAsync<MyStatefulObject, string>(key);

            Assert.Same(statefulObject, actualObject1);
            Assert.Null(cache.Get<object>(key));

            var actualObject2 = await sut.CreateAsync<MyStatefulObject, string>(key);

            A.CallTo(() => services.GetService(typeof(MyStatefulObject)))
                .MustHaveHappened(Repeated.Exactly.Twice);
        }

        [Fact]
        public async Task Should_write_to_store_with_previous_version()
        {
            statefulObject.ExpectedVersion = EtagVersion.Any;

            InvalidateMessage message = null;

            pubSub.Subscribe<InvalidateMessage>(m =>
            {
                message = m;
            });

            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((123, 13));

            var actualObject = await sut.GetSingleAsync<MyStatefulObject, string>(key);

            Assert.Same(statefulObject, actualObject);
            Assert.Equal(123, statefulObject.State);

            statefulObject.SetState(456);

            await statefulObject.WriteStateAsync();

            A.CallTo(() => snapshotStore.WriteAsync(key, 456, 13, 14))
                .MustHaveHappened();

            Assert.NotNull(message);
            Assert.Equal(key, message.Key);
        }

        [Fact]
        public async Task Should_wrap_exception_when_writing_to_store_with_previous_version()
        {
            statefulObject.ExpectedVersion = EtagVersion.Any;

            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((123, 13));

            A.CallTo(() => snapshotStore.WriteAsync(key, 123, 13, 14))
                .Throws(new InconsistentStateException(1, 1, new InvalidOperationException()));

            var actualObject = await sut.GetSingleAsync<MyStatefulObject, string>(key);

            await Assert.ThrowsAsync<DomainObjectVersionException>(() => statefulObject.WriteStateAsync());
        }

        [Fact]
        public async Task Should_remove_from_cache_when_invalidation_message_received()
        {
            statefulObject.ExpectedVersion = EtagVersion.Any;

            var actualObject = await sut.GetSingleAsync<MyStatefulObject, string>(key);

            await InvalidateCacheAsync();

            Assert.False(cache.TryGetValue(key, out var t));
        }

        [Fact]
        public async Task Should_return_same_instance_for_parallel_requests()
        {
            statefulObject.ExpectedVersion = EtagVersion.Any;

            A.CallTo(() => snapshotStore.ReadAsync(key))
                .ReturnsLazily(() => Task.Delay(1).ContinueWith(x => (1, 1L)));

            var tasks = new List<Task<MyStatefulObject>>();

            for (var i = 0; i < 1000; i++)
            {
                tasks.Add(Task.Run(() => sut.GetSingleAsync<MyStatefulObject, string>(key)));
            }

            var retrievedStates = await Task.WhenAll(tasks);

            foreach (var retrievedState in retrievedStates)
            {
                Assert.Same(retrievedStates[0], retrievedState);
            }

            A.CallTo(() => snapshotStore.ReadAsync(key))
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