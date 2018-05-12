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
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.States
{
    public class PersistenceEventSourcingTests
    {
        private readonly string key = Guid.NewGuid().ToString();
        private readonly IEventDataFormatter eventDataFormatter = A.Fake<IEventDataFormatter>();
        private readonly IEventStore eventStore = A.Fake<IEventStore>();
        private readonly IServiceProvider services = A.Fake<IServiceProvider>();
        private readonly ISnapshotStore<object, string> snapshotStore = A.Fake<ISnapshotStore<object, string>>();
        private readonly IStreamNameResolver streamNameResolver = A.Fake<IStreamNameResolver>();
        private readonly IStore<string> sut;

        public PersistenceEventSourcingTests()
        {
            A.CallTo(() => services.GetService(typeof(ISnapshotStore<object, string>)))
                .Returns(snapshotStore);

            A.CallTo(() => streamNameResolver.GetStreamName(typeof(object), key))
                .Returns(key);

            sut = new Store<string>(eventStore, eventDataFormatter, services, streamNameResolver);
        }

        [Fact]
        public async Task Should_read_from_store()
        {
            var event1 = new MyEvent();
            var event2 = new MyEvent();

            SetupEventStore(event1, event2);

            var persistedEvents = new List<IEvent>();
            var persistence = sut.WithEventSourcing<object, string>(key, x => persistedEvents.Add(x.Payload));

            await persistence.ReadAsync();

            Assert.Equal(persistedEvents.ToArray(), new[] { event1, event2 });
        }

        [Fact]
        public async Task Should_ignore_old_events()
        {
            var storedEvent = new StoredEvent("1", 0, new EventData());

            A.CallTo(() => eventStore.QueryAsync(key, 0))
                .Returns(new List<StoredEvent> { storedEvent });

            A.CallTo(() => eventDataFormatter.Parse(storedEvent.Data, true))
                .Throws(new TypeNameNotFoundException());

            var persistedEvents = new List<IEvent>();
            var persistence = sut.WithEventSourcing<object, string>(key, x => persistedEvents.Add(x.Payload));

            await persistence.ReadAsync();

            Assert.Empty(persistedEvents);
            Assert.Equal(0, persistence.Version);
        }

        [Fact]
        public async Task Should_read_status_from_snapshot()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((2, 2L));

            SetupEventStore(3, 2);

            var persistedState = (object)null;
            var persistedEvents = new List<IEvent>();
            var persistence = sut.WithSnapshotsAndEventSourcing<object, object, string>(key, x => persistedState = x, x => persistedEvents.Add(x.Payload));

            await persistence.ReadAsync();

            A.CallTo(() => eventStore.QueryAsync(key, 3))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_throw_exception_if_events_are_older_than_snapshot()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((2, 2L));

            SetupEventStore(3, 0, 3);

            var persistedState = (object)null;
            var persistedEvents = new List<IEvent>();
            var persistence = sut.WithSnapshotsAndEventSourcing<object, object, string>(key, x => persistedState = x, x => persistedEvents.Add(x.Payload));

            await Assert.ThrowsAsync<InvalidOperationException>(() => persistence.ReadAsync());
        }

        [Fact]
        public async Task Should_throw_exception_if_events_have_gaps_to_snapshot()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((2, 2L));

            SetupEventStore(3, 4, 3);

            var persistedState = (object)null;
            var persistedEvents = new List<IEvent>();
            var persistence = sut.WithSnapshotsAndEventSourcing<object, object, string>(key, x => persistedState = x, x => persistedEvents.Add(x.Payload));

            await Assert.ThrowsAsync<InvalidOperationException>(() => persistence.ReadAsync());
        }

        [Fact]
        public async Task Should_throw_exception_if_not_found()
        {
            SetupEventStore(0);

            var persistedEvents = new List<IEvent>();
            var persistence = sut.WithEventSourcing<object, string>(key, x => persistedEvents.Add(x.Payload));

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => persistence.ReadAsync(1));
        }

        [Fact]
        public async Task Should_throw_exception_if_other_version_found()
        {
            SetupEventStore(3);

            var persistedEvents = new List<IEvent>();
            var persistence = sut.WithEventSourcing<object, string>(key, x => persistedEvents.Add(x.Payload));

            await Assert.ThrowsAsync<DomainObjectVersionException>(() => persistence.ReadAsync(1));
        }

        [Fact]
        public async Task Should_throw_exception_if_other_version_found_from_snapshot()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((2, 2L));

            SetupEventStore(0);

            var persistedState = (object)null;
            var persistedEvents = new List<IEvent>();
            var persistence = sut.WithSnapshotsAndEventSourcing<object, object, string>(key, x => persistedState = x, x => persistedEvents.Add(x.Payload));

            await Assert.ThrowsAsync<DomainObjectVersionException>(() => persistence.ReadAsync(1));
        }

        [Fact]
        public async Task Should_not_throw_exception_if_nothing_expected()
        {
            SetupEventStore(0);

            var persistedState = (object)null;
            var persistedEvents = new List<IEvent>();
            var persistence = sut.WithSnapshotsAndEventSourcing<object, object, string>(key, x => persistedState = x, x => persistedEvents.Add(x.Payload));

            await persistence.ReadAsync();
        }

        [Fact]
        public async Task Should_write_to_store_with_previous_position()
        {
            SetupEventStore(3);

            var persistedEvents = new List<IEvent>();
            var persistence = sut.WithEventSourcing<object, string>(key, x => persistedEvents.Add(x.Payload));

            await persistence.ReadAsync();

            await persistence.WriteEventsAsync(new[] { new MyEvent(), new MyEvent() }.Select(Envelope.Create));
            await persistence.WriteEventsAsync(new[] { new MyEvent(), new MyEvent() }.Select(Envelope.Create));

            A.CallTo(() => eventStore.AppendAsync(A<Guid>.Ignored, key, 2, A<ICollection<EventData>>.That.Matches(x => x.Count == 2)))
                .MustHaveHappened();
            A.CallTo(() => eventStore.AppendAsync(A<Guid>.Ignored, key, 4, A<ICollection<EventData>>.That.Matches(x => x.Count == 2)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_wrap_exception_when_writing_to_store_with_previous_position()
        {
            SetupEventStore(3);

            var persistedEvents = new List<IEvent>();
            var persistence = sut.WithEventSourcing<object, string>(key, x => persistedEvents.Add(x.Payload));

            await persistence.ReadAsync();

            A.CallTo(() => eventStore.AppendAsync(A<Guid>.Ignored, key, 2, A<ICollection<EventData>>.That.Matches(x => x.Count == 2)))
                .Throws(new WrongEventVersionException(1, 1));

            await Assert.ThrowsAsync<DomainObjectVersionException>(() => persistence.WriteEventsAsync(new[] { new MyEvent(), new MyEvent() }.Select(Envelope.Create)));
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