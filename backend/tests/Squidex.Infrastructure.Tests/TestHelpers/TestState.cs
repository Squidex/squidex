// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;

namespace Squidex.Infrastructure.TestHelpers;

public sealed class TestState<T> where T : class, new()
{
    private readonly List<Envelope<IEvent>> events = new List<Envelope<IEvent>>();
    private readonly ISnapshotStore<T> snapshotStore = A.Fake<ISnapshotStore<T>>();
    private HandleSnapshot<T>? handleSnapshot;
    private HandleEvent? handleEvent;

    public DomainId Id { get; }

    public IPersistenceFactory<T> PersistenceFactory { get; }

    public IPersistence<T> Persistence { get; } = A.Fake<IPersistence<T>>();

    public long Version { get; set; } = EtagVersion.Empty;

    public T Snapshot { get; set; } = new T();

    public TestState(string id, IPersistenceFactory<T>? persistenceFactory = null)
        : this(DomainId.Create(id), persistenceFactory)
    {
    }

    public TestState(DomainId id, IPersistenceFactory<T>? persistenceFactory = null)
    {
        Id = id;

        PersistenceFactory = persistenceFactory ?? A.Fake<IPersistenceFactory<T>>();

        A.CallTo(() => PersistenceFactory.Snapshots)
            .Returns(snapshotStore);

        A.CallTo(() => Persistence.Version)
            .ReturnsLazily(() => Version);

        A.CallTo(() => snapshotStore.ReadAllAsync(A<CancellationToken>._))
            .ReturnsLazily(() => new List<SnapshotResult<T>>
            {
                new SnapshotResult<T>(id, Snapshot, Version, true)
            }.ToAsyncEnumerable());

        A.CallTo(() => PersistenceFactory.WithEventSourcing(A<Type>._, id, A<HandleEvent>._))
            .Invokes(x =>
            {
                handleEvent = x.GetArgument<HandleEvent>(2);
            })
            .Returns(Persistence);

        A.CallTo(() => PersistenceFactory.WithSnapshots(A<Type>._, id, A<HandleSnapshot<T>>._))
            .Invokes(x =>
            {
                handleSnapshot = x.GetArgument<HandleSnapshot<T>>(2);
            })
            .Returns(Persistence);

        A.CallTo(() => PersistenceFactory.WithSnapshotsAndEventSourcing(A<Type>._, id, A<HandleSnapshot<T>>._, A<HandleEvent>._))
            .Invokes(x =>
            {
                handleSnapshot = x.GetArgument<HandleSnapshot<T>>(2);
                handleEvent = x.GetArgument<HandleEvent>(3);
            })
            .Returns(Persistence);

        A.CallTo(() => Persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>._, A<CancellationToken>._))
            .Invokes(x =>
            {
                events.AddRange(x.GetArgument<IReadOnlyList<Envelope<IEvent>>>(0)!);
            });

        A.CallTo(() => Persistence.WriteSnapshotAsync(A<T>._, A<CancellationToken>._))
            .Invokes(x =>
            {
                Snapshot = x.GetArgument<T>(0)!;
            });

        A.CallTo(() => Persistence.ReadAsync(A<long>._, A<CancellationToken>._))
            .Invokes(x =>
            {
                handleSnapshot?.Invoke(Snapshot, Version);

                if (handleEvent != null)
                {
                    foreach (var @event in events)
                    {
                        handleEvent(@event);
                    }
                }
            });

        A.CallTo(() => Persistence.DeleteAsync(A<CancellationToken>._))
            .Invokes(x =>
            {
                Snapshot = new T();
            });
    }

    public void AddEvent(Envelope<IEvent> @event)
    {
        events.Add(@event);
    }

    public void AddEvent(IEvent @event)
    {
        events.Add(Envelope.Create(@event));
    }
}
