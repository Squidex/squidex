// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.States
{
    public class PersistenceEventSourcingTests
    {
        private class MyStatefulObject : IStatefulObject<string>
        {
            private readonly List<IEvent> appliedEvents = new List<IEvent>();
            private IPersistence persistence;

            public long ExpectedVersion { get; set; } = EtagVersion.Any;

            public List<IEvent> AppliedEvents
            {
                get { return appliedEvents; }
            }

            public Task ActivateAsync(string key, IStore<string> store)
            {
                persistence = store.WithEventSourcing(key, e => appliedEvents.Add(e.Payload));

                return persistence.ReadAsync(ExpectedVersion);
            }

            public Task WriteEventsAsync(params IEvent[] events)
            {
                return persistence.WriteEventsAsync(events.Select(Envelope.Create).ToArray());
            }
        }

        private class MyStatefulObjectWithSnapshot : IStatefulObject<string>
        {
            private IPersistence<object> persistence;

            public long ExpectedVersion { get; set; } = EtagVersion.Any;

            public Task ActivateAsync(string key, IStore<string> store)
            {
                persistence = store.WithSnapshotsAndEventSourcing<object>(key, s => TaskHelper.Done, s => TaskHelper.Done);

                return persistence.ReadAsync(ExpectedVersion);
            }
        }

        private readonly string key = Guid.NewGuid().ToString();
        private readonly MyStatefulObject statefulObject = new MyStatefulObject();
        private readonly MyStatefulObjectWithSnapshot statefulObjectWithSnapShot = new MyStatefulObjectWithSnapshot();
        private readonly IEventDataFormatter eventDataFormatter = A.Fake<IEventDataFormatter>();
        private readonly IEventStore eventStore = A.Fake<IEventStore>();
        private readonly IMemoryCache cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        private readonly IPubSub pubSub = new InMemoryPubSub(true);
        private readonly IServiceProvider services = A.Fake<IServiceProvider>();
        private readonly ISnapshotStore<object, string> snapshotStore = A.Fake<ISnapshotStore<object, string>>();
        private readonly IStreamNameResolver streamNameResolver = A.Fake<IStreamNameResolver>();
        private readonly StateFactory sut;

        public PersistenceEventSourcingTests()
        {
            A.CallTo(() => services.GetService(typeof(MyStatefulObject)))
                .Returns(statefulObject);
            A.CallTo(() => services.GetService(typeof(MyStatefulObjectWithSnapshot)))
                .Returns(statefulObjectWithSnapShot);
            A.CallTo(() => services.GetService(typeof(ISnapshotStore<object, string>)))
                .Returns(snapshotStore);

            A.CallTo(() => streamNameResolver.GetStreamName(typeof(MyStatefulObject), key))
                .Returns(key);
            A.CallTo(() => streamNameResolver.GetStreamName(typeof(MyStatefulObjectWithSnapshot), key))
                .Returns(key);

            sut = new StateFactory(pubSub, cache, eventStore, eventDataFormatter, services, streamNameResolver);
            sut.Initialize();
        }

        [Fact]
        public async Task Should_read_from_store()
        {
            statefulObject.ExpectedVersion = 1;

            var event1 = new MyEvent();
            var event2 = new MyEvent();

            SetupEventStore(event1, event2);

            var actualObject = await sut.GetSingleAsync<MyStatefulObject>(key);

            Assert.Same(statefulObject, actualObject);
            Assert.NotNull(cache.Get<object>(key));

            Assert.Equal(actualObject.AppliedEvents, new[] { event1, event2 });
        }

        [Fact]
        public async Task Should_read_status_from_snapshot()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((2, 2L));

            SetupEventStore(3, 2);

            await sut.GetSingleAsync<MyStatefulObjectWithSnapshot>(key);

            A.CallTo(() => eventStore.QueryAsync(key, 3))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_throw_exception_if_events_are_older_than_snapshot()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((2, 2L));

            SetupEventStore(3, 0, 3);

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.GetSingleAsync<MyStatefulObjectWithSnapshot>(key));
        }

        [Fact]
        public async Task Should_throw_exception_if_events_have_gaps_to_snapshot()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((2, 2L));

            SetupEventStore(3, 4, 3);

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.GetSingleAsync<MyStatefulObjectWithSnapshot>(key));
        }

        [Fact]
        public async Task Should_throw_exception_if_not_found()
        {
            statefulObject.ExpectedVersion = 0;

            SetupEventStore(0);

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.GetSingleAsync<MyStatefulObject>(key));
        }

        [Fact]
        public async Task Should_throw_exception_if_other_version_found()
        {
            statefulObject.ExpectedVersion = 1;

            SetupEventStore(3);

            await Assert.ThrowsAsync<DomainObjectVersionException>(() => sut.GetSingleAsync<MyStatefulObject>(key));
        }

        [Fact]
        public async Task Should_throw_exception_if_other_version_found_from_snapshot()
        {
            statefulObjectWithSnapShot.ExpectedVersion = 1;

            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((2, 2L));

            SetupEventStore(0);

            await Assert.ThrowsAsync<DomainObjectVersionException>(() => sut.GetSingleAsync<MyStatefulObjectWithSnapshot>(key));
        }

        [Fact]
        public async Task Should_not_throw_exception_if_nothing_expected()
        {
            statefulObject.ExpectedVersion = EtagVersion.Any;

            SetupEventStore(0);

            await sut.GetSingleAsync<MyStatefulObject>(key);
        }

        [Fact]
        public async Task Should_write_to_store_with_previous_position()
        {
            SetupEventStore(3);

            var actualObject = await sut.GetSingleAsync<MyStatefulObject>(key);

            Assert.Same(statefulObject, actualObject);

            await statefulObject.WriteEventsAsync(new MyEvent(), new MyEvent());
            await statefulObject.WriteEventsAsync(new MyEvent(), new MyEvent());

            A.CallTo(() => eventStore.AppendAsync(A<Guid>.Ignored, key, 2, A<ICollection<EventData>>.That.Matches(x => x.Count == 2)))
                .MustHaveHappened();
            A.CallTo(() => eventStore.AppendAsync(A<Guid>.Ignored, key, 4, A<ICollection<EventData>>.That.Matches(x => x.Count == 2)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_wrap_exception_when_writing_to_store_with_previous_position()
        {
            SetupEventStore(3);

            var actualObject = await sut.GetSingleAsync<MyStatefulObject>(key);

            A.CallTo(() => eventStore.AppendAsync(A<Guid>.Ignored, key, 2, A<ICollection<EventData>>.That.Matches(x => x.Count == 2)))
                .Throws(new WrongEventVersionException(1, 1));

            await Assert.ThrowsAsync<DomainObjectVersionException>(() => statefulObject.WriteEventsAsync(new MyEvent(), new MyEvent()));
        }

        [Fact]
        public async Task Should_not_remove_from_cache_when_write_failed()
        {
            A.CallTo(() => eventStore.AppendAsync(A<Guid>.Ignored, A<string>.Ignored, A<long>.Ignored, A<ICollection<EventData>>.Ignored))
                .Throws(new InvalidOperationException());

            var actualObject = await sut.GetSingleAsync<MyStatefulObject>(key);

            await Assert.ThrowsAsync<InvalidOperationException>(() => statefulObject.WriteEventsAsync(new MyEvent()));

            Assert.True(cache.TryGetValue(key, out var t));
        }

        [Fact]
        public async Task Should_return_same_instance_for_parallel_requests()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key))
                .ReturnsLazily(() => Task.Delay(1).ContinueWith(x => ((object)1, 1L)));

            var tasks = new List<Task<MyStatefulObject>>();

            for (var i = 0; i < 1000; i++)
            {
                tasks.Add(Task.Run(() => sut.GetSingleAsync<MyStatefulObject>(key)));
            }

            var retrievedStates = await Task.WhenAll(tasks);

            foreach (var retrievedState in retrievedStates)
            {
                Assert.Same(retrievedStates[0], retrievedState);
            }

            A.CallTo(() => eventStore.QueryAsync(key, 0))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        private void SetupEventStore(int count, int eventOffset = 0, int readPosition = 0)
        {
            SetupEventStore(Enumerable.Repeat(0, count).Select(x => new MyEvent()).ToArray(), eventOffset, readPosition);
        }

        private void SetupEventStore(params MyEvent[] events)
        {
            SetupEventStore(events, 0, 0);
        }

        private void SetupEventStore(MyEvent[] events, int eventOffset = 0, int readPosition = 0)
        {
            var eventsStored = new List<StoredEvent>();

            var i = eventOffset;

            foreach (var @event in events)
            {
                var eventData = new EventData();
                var eventStored = new StoredEvent(i.ToString(), i, eventData);

                eventsStored.Add(eventStored);

                A.CallTo(() => eventDataFormatter.Parse(eventData, true))
                    .Returns(new Envelope<IEvent>(@event));

                i++;
            }

            A.CallTo(() => eventStore.QueryAsync(key, readPosition))
                .Returns(eventsStored);
        }
    }
}