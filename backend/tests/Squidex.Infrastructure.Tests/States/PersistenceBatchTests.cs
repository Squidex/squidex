// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.States
{
    public class PersistenceBatchTests
    {
        private readonly ISnapshotStore<int> snapshotStore = A.Fake<ISnapshotStore<int>>();
        private readonly IEventDataFormatter eventDataFormatter = A.Fake<IEventDataFormatter>();
        private readonly IEventStore eventStore = A.Fake<IEventStore>();
        private readonly IStreamNameResolver streamNameResolver = A.Fake<IStreamNameResolver>();
        private readonly IStore<int> sut;

        public PersistenceBatchTests()
        {
            A.CallTo(() => streamNameResolver.GetStreamName(None.Type, A<string>._))
                .ReturnsLazily(x => x.GetArgument<string>(1)!);

            sut = new Store<int>(snapshotStore, eventStore, eventDataFormatter, streamNameResolver);
        }

        [Fact]
        public async Task Should_read_from_preloaded_events()
        {
            var event1_1 = new MyEvent { MyProperty = "event1_1" };
            var event1_2 = new MyEvent { MyProperty = "event1_2" };
            var event2_1 = new MyEvent { MyProperty = "event2_1" };
            var event2_2 = new MyEvent { MyProperty = "event2_2" };

            var key1 = DomainId.NewGuid();
            var key2 = DomainId.NewGuid();

            var bulk = sut.WithBatchContext(None.Type);

            SetupEventStore(new Dictionary<DomainId, List<MyEvent>>
            {
                [key1] = new List<MyEvent> { event1_1, event1_2 },
                [key2] = new List<MyEvent> { event2_1, event2_2 }
            });

            await bulk.LoadAsync(new[] { key1, key2 });

            var persistedEvents1 = Save.Events();
            var persistence1 = bulk.WithEventSourcing(None.Type, key1, persistedEvents1.Write);

            await persistence1.ReadAsync();

            var persistedEvents2 = Save.Events();
            var persistence2 = bulk.WithEventSourcing(None.Type, key2, persistedEvents2.Write);

            await persistence2.ReadAsync();

            Assert.Equal(persistedEvents1.ToArray(), new[] { event1_1, event1_2 });
            Assert.Equal(persistedEvents2.ToArray(), new[] { event2_1, event2_2 });
        }

        [Fact]
        public async Task Should_provide_empty_events_if_nothing_loaded()
        {
            var key = DomainId.NewGuid();

            var bulk = sut.WithBatchContext(None.Type);

            await bulk.LoadAsync(new[] { key });

            var persistedEvents = Save.Events();
            var persistence = bulk.WithEventSourcing(None.Type, key, persistedEvents.Write);

            await persistence.ReadAsync();

            Assert.Empty(persistedEvents.ToArray());
            Assert.Empty(persistedEvents.ToArray());
        }

        [Fact]
        public void Should_throw_exception_if_not_preloaded()
        {
            var key = DomainId.NewGuid();

            var bulk = sut.WithBatchContext(None.Type);

            Assert.Throws<KeyNotFoundException>(() => bulk.WithEventSourcing(None.Type, key, null));
        }

        [Fact]
        public async Task Should_write_batched()
        {
            var key1 = DomainId.NewGuid();
            var key2 = DomainId.NewGuid();

            var bulk = sut.WithBatchContext(None.Type);

            await bulk.LoadAsync(new[] { key1, key2 });

            var persistedEvents1 = Save.Events();
            var persistence1 = bulk.WithEventSourcing(None.Type, key1, persistedEvents1.Write);

            var persistedEvents2 = Save.Events();
            var persistence2 = bulk.WithEventSourcing(None.Type, key2, persistedEvents2.Write);

            await persistence1.WriteSnapshotAsync(12);
            await persistence2.WriteSnapshotAsync(12);

            A.CallTo(() => snapshotStore.WriteAsync(A<DomainId>._, A<int>._, A<long>._, A<long>._, A<CancellationToken>._))
                .MustNotHaveHappened();

            A.CallTo(() => snapshotStore.WriteManyAsync(A<IEnumerable<(DomainId, int, long)>>._, A<CancellationToken>._))
                .MustNotHaveHappened();

            await bulk.CommitAsync();
            await bulk.DisposeAsync();

            A.CallTo(() => snapshotStore.WriteManyAsync(A<IEnumerable<(DomainId, int, long)>>.That.Matches(x => x.Count() == 2), A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_write_each_id_only_once_if_same_id_requested_twice()
        {
            var key1 = DomainId.NewGuid();
            var key2 = DomainId.NewGuid();

            var bulk = sut.WithBatchContext(None.Type);

            await bulk.LoadAsync(new[] { key1, key2 });

            var persistedEvents1_1 = Save.Events();
            var persistence1_1 = bulk.WithEventSourcing(None.Type, key1, persistedEvents1_1.Write);

            var persistedEvents1_2 = Save.Events();
            var persistence1_2 = bulk.WithEventSourcing(None.Type, key1, persistedEvents1_2.Write);

            await persistence1_1.WriteSnapshotAsync(12);
            await persistence1_2.WriteSnapshotAsync(12);

            A.CallTo(() => snapshotStore.WriteAsync(A<DomainId>._, A<int>._, A<long>._, A<long>._, A<CancellationToken>._))
                .MustNotHaveHappened();

            A.CallTo(() => snapshotStore.WriteManyAsync(A<IEnumerable<(DomainId, int, long)>>._, A<CancellationToken>._))
                .MustNotHaveHappened();

            await bulk.CommitAsync();
            await bulk.DisposeAsync();

            A.CallTo(() => snapshotStore.WriteManyAsync(A<IEnumerable<(DomainId, int, long)>>.That.Matches(x => x.Count() == 1), A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_write_each_id_only_once_if_same_persistence_written_twice()
        {
            var key1 = DomainId.NewGuid();
            var key2 = DomainId.NewGuid();

            var bulk = sut.WithBatchContext(None.Type);

            await bulk.LoadAsync(new[] { key1, key2 });

            var persistedEvents1 = Save.Events();
            var persistence1 = bulk.WithEventSourcing(None.Type, key1, persistedEvents1.Write);

            await persistence1.WriteSnapshotAsync(12);
            await persistence1.WriteSnapshotAsync(13);

            A.CallTo(() => snapshotStore.WriteAsync(A<DomainId>._, A<int>._, A<long>._, A<long>._, A<CancellationToken>._))
                .MustNotHaveHappened();

            A.CallTo(() => snapshotStore.WriteManyAsync(A<IEnumerable<(DomainId, int, long)>>._, A<CancellationToken>._))
                .MustNotHaveHappened();

            await bulk.CommitAsync();
            await bulk.DisposeAsync();

            A.CallTo(() => snapshotStore.WriteManyAsync(A<IEnumerable<(DomainId, int, long)>>.That.Matches(x => x.Count() == 1), A<CancellationToken>._))
                .MustHaveHappenedOnceExactly();
        }

        private void SetupEventStore(Dictionary<DomainId, List<MyEvent>> streams)
        {
            var storedStreams = new Dictionary<string, IReadOnlyList<StoredEvent>>();

            foreach (var (id, stream) in streams)
            {
                var storedStream = new List<StoredEvent>();

                var i = 0;

                foreach (var @event in stream)
                {
                    var eventData = new EventData("Type", new EnvelopeHeaders(), "Payload");
                    var eventStored = new StoredEvent(id.ToString(), i.ToString(CultureInfo.InvariantCulture), i, eventData);

                    storedStream.Add(eventStored);

                    A.CallTo(() => eventDataFormatter.Parse(eventStored))
                        .Returns(new Envelope<IEvent>(@event));

                    A.CallTo(() => eventDataFormatter.ParseIfKnown(eventStored))
                        .Returns(new Envelope<IEvent>(@event));

                    i++;
                }

                storedStreams[id.ToString()] = storedStream;
            }

            var streamNames = streams.Keys.Select(x => x.ToString()).ToArray();

            A.CallTo(() => eventStore.QueryManyAsync(A<IEnumerable<string>>.That.IsSameSequenceAs(streamNames), A<CancellationToken>._))
                .Returns(storedStreams);
        }
    }
}
