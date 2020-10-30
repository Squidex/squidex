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
        private readonly IEventEnricher<string> eventEnricher = A.Fake<IEventEnricher<string>>();
        private readonly IEventDataFormatter eventDataFormatter = A.Fake<IEventDataFormatter>();
        private readonly IEventStore eventStore = A.Fake<IEventStore>();
        private readonly IServiceProvider services = A.Fake<IServiceProvider>();
        private readonly ISnapshotStore<int, string> snapshotStore = A.Fake<ISnapshotStore<int, string>>();
        private readonly ISnapshotStore<None, string> snapshotStoreNone = A.Fake<ISnapshotStore<None, string>>();
        private readonly IStreamNameResolver streamNameResolver = A.Fake<IStreamNameResolver>();
        private readonly IStore<string> sut;

        public PersistenceEventSourcingTests()
        {
            A.CallTo(() => services.GetService(typeof(ISnapshotStore<int, string>)))
                .Returns(snapshotStore);
            A.CallTo(() => services.GetService(typeof(ISnapshotStore<None, string>)))
                .Returns(snapshotStoreNone);

            A.CallTo(() => streamNameResolver.GetStreamName(None.Type, key))
                .Returns(key);

            sut = new Store<string>(eventStore, eventEnricher, eventDataFormatter, services, streamNameResolver);
        }

        [Fact]
        public async Task Should_read_from_store()
        {
            var event1 = new MyEvent();
            var event2 = new MyEvent();

            SetupEventStore(event1, event2);

            var persistedEvents = new List<IEvent>();
            var persistence = sut.WithEventSourcing(None.Type, key, x => persistedEvents.Add(x.Payload));

            await persistence.ReadAsync();

            Assert.Equal(persistedEvents.ToArray(), new[] { event1, event2 });
        }

        [Fact]
        public async Task Should_ignore_old_events()
        {
            var storedEvent = new StoredEvent("1", "1", 0, new EventData("Type", new EnvelopeHeaders(), "Payload"));

            A.CallTo(() => eventStore.QueryAsync(key, 0))
                .Returns(new List<StoredEvent> { storedEvent });

            A.CallTo(() => eventDataFormatter.ParseIfKnown(storedEvent))
                .Returns(null);

            var persistedEvents = new List<IEvent>();
            var persistence = sut.WithEventSourcing(None.Type, key, x => persistedEvents.Add(x.Payload));

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

            var persistedState = -1;
            var persistedEvents = new List<IEvent>();
            var persistence = sut.WithSnapshotsAndEventSourcing(None.Type, key, (int x) => persistedState = x, x => persistedEvents.Add(x.Payload));

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

            var persistedState = -1;
            var persistedEvents = new List<IEvent>();
            var persistence = sut.WithSnapshotsAndEventSourcing(None.Type, key, (int x) => persistedState = x, x => persistedEvents.Add(x.Payload));

            await Assert.ThrowsAsync<InvalidOperationException>(() => persistence.ReadAsync());
        }

        [Fact]
        public async Task Should_throw_exception_if_events_have_gaps_to_snapshot()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((2, 2L));

            SetupEventStore(3, 4, 3);

            var persistedState = -1;
            var persistedEvents = new List<IEvent>();
            var persistence = sut.WithSnapshotsAndEventSourcing(None.Type, key, (int x) => persistedState = x, x => persistedEvents.Add(x.Payload));

            await Assert.ThrowsAsync<InvalidOperationException>(() => persistence.ReadAsync());
        }

        [Fact]
        public async Task Should_throw_exception_if_not_found()
        {
            SetupEventStore(0);

            var persistedEvents = new List<IEvent>();
            var persistence = sut.WithEventSourcing(None.Type, key, x => persistedEvents.Add(x.Payload));

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => persistence.ReadAsync(1));
        }

        [Fact]
        public async Task Should_throw_exception_if_other_version_found()
        {
            SetupEventStore(3);

            var persistedEvents = new List<IEvent>();
            var persistence = sut.WithEventSourcing(None.Type, key, x => persistedEvents.Add(x.Payload));

            await Assert.ThrowsAsync<InconsistentStateException>(() => persistence.ReadAsync(1));
        }

        [Fact]
        public async Task Should_throw_exception_if_other_version_found_from_snapshot()
        {
            A.CallTo(() => snapshotStore.ReadAsync(key))
                .Returns((2, 2L));

            SetupEventStore(0);

            var persistedState = -1;
            var persistedEvents = new List<IEvent>();
            var persistence = sut.WithSnapshotsAndEventSourcing(None.Type, key, (int x) => persistedState = x, x => persistedEvents.Add(x.Payload));

            await Assert.ThrowsAsync<InconsistentStateException>(() => persistence.ReadAsync(1));
        }

        [Fact]
        public async Task Should_not_throw_exception_if_nothing_expected()
        {
            SetupEventStore(0);

            var persistedState = -1;
            var persistedEvents = new List<IEvent>();
            var persistence = sut.WithSnapshotsAndEventSourcing(None.Type, key, (int x) => persistedState = x, x => persistedEvents.Add(x.Payload));

            await persistence.ReadAsync();
        }

        [Fact]
        public async Task Should_write_to_store_with_previous_version()
        {
            SetupEventStore(3);

            var persistedEvents = new List<IEvent>();
            var persistence = sut.WithEventSourcing(None.Type, key, x => persistedEvents.Add(x.Payload));

            await persistence.ReadAsync();

            await persistence.WriteEventAsync(Envelope.Create(new MyEvent()));
            await persistence.WriteEventAsync(Envelope.Create(new MyEvent()));

            A.CallTo(() => eventStore.AppendAsync(A<Guid>._, key, 2, A<ICollection<EventData>>.That.Matches(x => x.Count == 1)))
                .MustHaveHappened();
            A.CallTo(() => eventStore.AppendAsync(A<Guid>._, key, 3, A<ICollection<EventData>>.That.Matches(x => x.Count == 1)))
                .MustHaveHappened();
            A.CallTo(() => eventEnricher.Enrich(A<Envelope<IEvent>>._, key))
                .MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task Should_write_events_to_store_with_empty_version()
        {
            var persistence = sut.WithEventSourcing(None.Type, key, null);

            await persistence.WriteEventAsync(Envelope.Create(new MyEvent()));

            A.CallTo(() => eventStore.AppendAsync(A<Guid>._, key, EtagVersion.Empty, A<ICollection<EventData>>.That.Matches(x => x.Count == 1)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_wrap_exception_when_writing_to_store_with_previous_version()
        {
            SetupEventStore(3);

            var persistedEvents = new List<IEvent>();
            var persistence = sut.WithEventSourcing(None.Type, key, x => persistedEvents.Add(x.Payload));

            await persistence.ReadAsync();

            A.CallTo(() => eventStore.AppendAsync(A<Guid>._, key, 2, A<ICollection<EventData>>.That.Matches(x => x.Count == 1)))
                .Throws(new WrongEventVersionException(1, 1));

            await Assert.ThrowsAsync<InconsistentStateException>(() => persistence.WriteEventAsync(Envelope.Create(new MyEvent())));
        }

        [Fact]
        public async Task Should_delete_events_but_not_snapshot_when_deleted_snapshot_only()
        {
            var persistence = sut.WithEventSourcing(None.Type, key, null);

            await persistence.DeleteAsync();

            A.CallTo(() => eventStore.DeleteStreamAsync(key))
                .MustHaveHappened();

            A.CallTo(() => snapshotStore.RemoveAsync(key))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_delete_events_and_snapshot_when_deleted()
        {
            var persistence = sut.WithSnapshotsAndEventSourcing<int>(None.Type, key, null, null);

            await persistence.DeleteAsync();

            A.CallTo(() => eventStore.DeleteStreamAsync(key))
                .MustHaveHappened();

            A.CallTo(() => snapshotStore.RemoveAsync(key))
                .MustHaveHappened();
        }

        private void SetupEventStore(int count, int eventOffset = 0, int readPosition = 0)
        {
            SetupEventStore(Enumerable.Repeat(0, count).Select(x => new MyEvent()).ToArray(), eventOffset, readPosition);
        }

        private void SetupEventStore(params MyEvent[] events)
        {
            SetupEventStore(events, 0, 0);
        }

        private void SetupEventStore(MyEvent[] events, int eventOffset, int readPosition = 0)
        {
            var eventsStored = new List<StoredEvent>();

            var i = eventOffset;

            foreach (var @event in events)
            {
                var eventData = new EventData("Type", new EnvelopeHeaders(), "Payload");
                var eventStored = new StoredEvent(i.ToString(), i.ToString(), i, eventData);

                eventsStored.Add(eventStored);

                A.CallTo(() => eventDataFormatter.Parse(eventStored))
                    .Returns(new Envelope<IEvent>(@event));

                A.CallTo(() => eventDataFormatter.ParseIfKnown(eventStored))
                    .Returns(new Envelope<IEvent>(@event));

                i++;
            }

            A.CallTo(() => eventStore.QueryAsync(key, readPosition))
                .Returns(eventsStored);
        }
    }
}