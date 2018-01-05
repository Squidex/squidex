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
    public class PersistenceSnapshotTests : IDisposable
    {
        private class MyStatefulObject : IStatefulObject<string>
        {
            private IPersistence<int> persistence;
            private int state;

            public long ExpectedVersion { get; set; } = EtagVersion.Any;

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

        public PersistenceSnapshotTests()
        {
            A.CallTo(() => services.GetService(typeof(MyStatefulObject)))
                .Returns(statefulObject);
            A.CallTo(() => services.GetService(typeof(ISnapshotStore<int, string>)))
                .Returns(snapshotStore);

            sut = new StateFactory(pubSub, cache, eventStore, eventDataFormatter, services, streamNameResolver);
            sut.Initialize();
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
            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((0, EtagVersion.Empty));

            await sut.GetSingleAsync<MyStatefulObject, string>(key);
        }

        [Fact]
        public async Task Should_write_to_store_with_previous_version()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((123, 13));

            var actualObject = await sut.GetSingleAsync<MyStatefulObject, string>(key);

            Assert.Same(statefulObject, actualObject);
            Assert.Equal(123, statefulObject.State);

            statefulObject.SetState(456);

            await statefulObject.WriteStateAsync();

            A.CallTo(() => snapshotStore.WriteAsync(key, 456, 13, 14))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_wrap_exception_when_writing_to_store_with_previous_version()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((123, 13));

            A.CallTo(() => snapshotStore.WriteAsync(key, 123, 13, 14))
                .Throws(new InconsistentStateException(1, 1, new InvalidOperationException()));

            var actualObject = await sut.GetSingleAsync<MyStatefulObject, string>(key);

            await Assert.ThrowsAsync<DomainObjectVersionException>(() => statefulObject.WriteStateAsync());
        }

        [Fact]
        public async Task Should_not_remove_from_cache_when_write_failed()
        {
            A.CallTo(() => snapshotStore.WriteAsync(A<string>.Ignored, A<int>.Ignored, A<long>.Ignored, A<long>.Ignored))
                .Throws(new InvalidOperationException());

            var actualObject = await sut.GetSingleAsync<MyStatefulObject>(key);

            await Assert.ThrowsAsync<InvalidOperationException>(() => statefulObject.WriteStateAsync());

            Assert.True(cache.TryGetValue(key, out var t));
        }

        [Fact]
        public async Task Should_return_same_instance_for_parallel_requests()
        {
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
    }
}