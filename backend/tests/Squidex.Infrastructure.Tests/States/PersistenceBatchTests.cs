﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Squidex.Events;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Infrastructure.States;

public class PersistenceBatchTests
{
    private readonly IEventFormatter eventFormatter = A.Fake<IEventFormatter>();
    private readonly IEventStore eventStore = A.Fake<IEventStore>();
    private readonly IEventStreamNames eventStreamNames = A.Fake<IEventStreamNames>();
    private readonly ISnapshotStore<int> snapshotStore = A.Fake<ISnapshotStore<int>>();
    private readonly IStore<int> sut;

    public PersistenceBatchTests()
    {
        A.CallTo(() => eventStreamNames.GetStreamName(None.Type, A<string>._))
            .ReturnsLazily(x => x.GetArgument<string>(1)!);

        sut = new Store<int>(eventFormatter, eventStore, eventStreamNames, snapshotStore);
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
            [key1] = [event1_1, event1_2],
            [key2] = [event2_1, event2_2],
        });

        await bulk.LoadAsync([key1, key2]);

        var persistedEvents1 = Save.Events();
        var persistence1 = bulk.WithEventSourcing(None.Type, key1, persistedEvents1.Write);

        await persistence1.ReadAsync();

        var persistedEvents2 = Save.Events();
        var persistence2 = bulk.WithEventSourcing(None.Type, key2, persistedEvents2.Write);

        await persistence2.ReadAsync();

        Assert.Equal(persistedEvents1.ToArray(), [event1_1, event1_2]);
        Assert.Equal(persistedEvents2.ToArray(), [event2_1, event2_2]);
    }

    [Fact]
    public async Task Should_provide_empty_events_if_nothing_loaded()
    {
        var key = DomainId.NewGuid();

        var bulk = sut.WithBatchContext(None.Type);

        await bulk.LoadAsync([key]);

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

        await bulk.LoadAsync([key1, key2]);

        var persistedEvents1 = Save.Events();
        var persistence1 = bulk.WithEventSourcing(None.Type, key1, persistedEvents1.Write);

        var persistedEvents2 = Save.Events();
        var persistence2 = bulk.WithEventSourcing(None.Type, key2, persistedEvents2.Write);

        await persistence1.WriteSnapshotAsync(12);
        await persistence2.WriteSnapshotAsync(12);

        A.CallTo(() => snapshotStore.WriteAsync(A<SnapshotWriteJob<int>>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => snapshotStore.WriteManyAsync(A<IEnumerable<SnapshotWriteJob<int>>>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        await bulk.CommitAsync();
        await bulk.DisposeAsync();

        A.CallTo(() => snapshotStore.WriteManyAsync(A<IEnumerable<SnapshotWriteJob<int>>>.That.Matches(x => x.Count() == 2), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_write_each_id_only_once_if_same_id_requested_twice()
    {
        var key1 = DomainId.NewGuid();
        var key2 = DomainId.NewGuid();

        var bulk = sut.WithBatchContext(None.Type);

        await bulk.LoadAsync([key1, key2]);

        var persistedEvents1_1 = Save.Events();
        var persistence1_1 = bulk.WithEventSourcing(None.Type, key1, persistedEvents1_1.Write);

        var persistedEvents1_2 = Save.Events();
        var persistence1_2 = bulk.WithEventSourcing(None.Type, key1, persistedEvents1_2.Write);

        await persistence1_1.WriteSnapshotAsync(12);
        await persistence1_2.WriteSnapshotAsync(12);

        A.CallTo(() => snapshotStore.WriteAsync(A<SnapshotWriteJob<int>>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => snapshotStore.WriteManyAsync(A<IEnumerable<SnapshotWriteJob<int>>>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        await bulk.CommitAsync();
        await bulk.DisposeAsync();

        A.CallTo(() => snapshotStore.WriteManyAsync(A<IEnumerable<SnapshotWriteJob<int>>>.That.Matches(x => x.Count() == 1), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_write_each_id_only_once_if_same_persistence_written_twice()
    {
        var key1 = DomainId.NewGuid();
        var key2 = DomainId.NewGuid();

        var bulk = sut.WithBatchContext(None.Type);

        await bulk.LoadAsync([key1, key2]);

        var persistedEvents1 = Save.Events();
        var persistence1 = bulk.WithEventSourcing(None.Type, key1, persistedEvents1.Write);

        await persistence1.WriteSnapshotAsync(12);
        await persistence1.WriteSnapshotAsync(13);

        A.CallTo(() => snapshotStore.WriteAsync(A<SnapshotWriteJob<int>>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => snapshotStore.WriteManyAsync(A<IEnumerable<SnapshotWriteJob<int>>>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        await bulk.CommitAsync();
        await bulk.DisposeAsync();

        A.CallTo(() => snapshotStore.WriteManyAsync(A<IEnumerable<SnapshotWriteJob<int>>>.That.Matches(x => x.Count() == 1), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    private void SetupEventStore(Dictionary<DomainId, List<MyEvent>> streams)
    {
        var events = new List<StoredEvent>();

        foreach (var (id, stream) in streams)
        {
            var i = 0;

            foreach (var @event in stream)
            {
                var eventData = new EventData("Type", [], "Payload");
                var eventStored = new StoredEvent(id.ToString(), i.ToString(CultureInfo.InvariantCulture), i, eventData);

                A.CallTo(() => eventFormatter.Parse(eventStored))
                    .Returns(new Envelope<IEvent>(@event));

                A.CallTo(() => eventFormatter.ParseIfKnown(eventStored))
                    .Returns(new Envelope<IEvent>(@event));

                events.Add(eventStored);
                i++;
            }
        }

        var filter = StreamFilter.Name(streams.Keys.Select(x => x.ToString()).ToArray());

        A.CallTo(() => eventStore.QueryAllAsync(filter, null, int.MaxValue, A<CancellationToken>._))
            .Returns(events.ToAsyncEnumerable());
    }
}
