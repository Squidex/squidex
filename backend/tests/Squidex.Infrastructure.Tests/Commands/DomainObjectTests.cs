// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.TestHelpers;
using Xunit;

namespace Squidex.Infrastructure.Commands
{
    public class DomainObjectTests
    {
        private readonly IPersistenceFactory<MyDomainState> persistenceFactory = A.Fake<IPersistenceFactory<MyDomainState>>();
        private readonly IPersistence<MyDomainState> persistence = A.Fake<IPersistence<MyDomainState>>();
        private readonly IPersistence<MyDomainState> persistenceEvents = A.Fake<IPersistence<MyDomainState>>();
        private readonly DomainId id = DomainId.NewGuid();
        private readonly MyDomainObject sut;
        private HandleEvent? handleEvent;

        public DomainObjectTests()
        {
            A.CallTo(() => persistenceFactory.WithEventSourcing(typeof(MyDomainObject), id, A<HandleEvent>._))
                .Invokes(args =>
                {
                    handleEvent = args.GetArgument<HandleEvent>(2)!;
                })
                .Returns(persistenceEvents);

            A.CallTo(() => persistenceFactory.WithSnapshotsAndEventSourcing(typeof(MyDomainObject), id, A<HandleSnapshot<MyDomainState>>._, A<HandleEvent>._))
                .Invokes(args =>
                {
                    handleEvent = args.GetArgument<HandleEvent>(3)!;
                })
                .Returns(persistence);

            sut = new MyDomainObject(id, persistenceFactory);
        }

        [Fact]
        public void Should_instantiate()
        {
            Assert.Equal(EtagVersion.Empty, sut.Version);
            AssertSnapshot(sut.Snapshot, 0, EtagVersion.Empty);
        }

        [Fact]
        public async Task Should_repair_if_stale()
        {
            A.CallTo(() => persistence.IsSnapshotStale)
                .Returns(true);

            SetupCreated(1);

            await sut.EnsureLoadedAsync();

            A.CallTo(() => persistence.WriteSnapshotAsync(A<MyDomainState>._, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_repair_if_not_stale()
        {
            A.CallTo(() => persistence.IsSnapshotStale)
                .Returns(false);

            SetupCreated(1);

            await sut.EnsureLoadedAsync();

            A.CallTo(() => persistence.WriteSnapshotAsync(A<MyDomainState>._, default))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_write_state_and_events_if_created()
        {
            SetupEmpty();

            var result = await sut.ExecuteAsync(new CreateAuto { Value = 4 });

            A.CallTo(() => persistence.WriteSnapshotAsync(A<MyDomainState>.That.Matches(x => x.Value == 4), default))
                .MustHaveHappened();
            A.CallTo(() => persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>.That.Matches(x => x.Count == 1), default))
                .MustHaveHappened();
            A.CallTo(() => persistence.ReadAsync(A<long>._, default))
                .MustNotHaveHappened();

            Assert.Equal(CommandResult.Empty(id, 0, EtagVersion.Empty), result);
            Assert.Equal(0, sut.Version);
            Assert.Equal(0, sut.Snapshot.Version);

            Assert.Empty(sut.GetUncomittedEvents());
            AssertSnapshot(sut.Snapshot, 4, 0);
        }

        [Fact]
        public async Task Should_migrate_old_event_with_state()
        {
            SetupCreated(new ValueChanged { Value = 10 }, new MultipleByTwiceEvent());

            await sut.EnsureLoadedAsync();

            Assert.Equal(1, sut.Version);
            Assert.Equal(1, sut.Snapshot.Version);
            Assert.Equal(20, sut.Snapshot.Value);
        }

        [Fact]
        public async Task Should_recreate_when_loading()
        {
            sut.RecreateEvent = true;

            SetupCreated(
                new ValueChanged { Value = 2 },
                new ValueChanged { Value = 3 },
                new Deleted(),
                new ValueChanged { Value = 4 });

            await sut.EnsureLoadedAsync();

            Assert.Equal(3, sut.Version);
            Assert.Equal(3, sut.Snapshot.Version);

            AssertSnapshot(sut.Snapshot, 4, 3);
        }

        [Fact]
        public async Task Should_ignore_events_after_deleting_when_loading()
        {
            SetupCreated(
                new ValueChanged { Value = 2 },
                new ValueChanged { Value = 3 },
                new Deleted(),
                new ValueChanged { Value = 4 });

            await sut.EnsureLoadedAsync();

            Assert.Equal(2, sut.Version);
            Assert.Equal(2, sut.Snapshot.Version);

            AssertSnapshot(sut.Snapshot, 3, 2, true);
        }

        [Fact]
        public async Task Should_recreate_with_create_command_if_deleted_before()
        {
            sut.Recreate = true;
            sut.RecreateEvent = true;

            SetupCreated(2);
            SetupDeleted();

            A.CallTo(() => persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>._, default))
                .Throws(new InconsistentStateException(2, -1)).Once();

            var result = await sut.ExecuteAsync(new CreateAuto { Value = 4 });

            A.CallTo(() => persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>.That.Matches(x => x.Count == 1), default))
                .MustHaveHappenedANumberOfTimesMatching(x => x == 3);
            A.CallTo(() => persistence.ReadAsync(A<long>._, default))
                .MustHaveHappened();

            Assert.Equal(CommandResult.Empty(id, 2, 1), result);
            Assert.Equal(2, sut.Version);
            Assert.Equal(2, sut.Snapshot.Version);

            Assert.Empty(sut.GetUncomittedEvents());
            AssertSnapshot(sut.Snapshot, 4, 2);
        }

        [Fact]
        public async Task Should_throw_exception_if_recreation_with_create_command_not_allowed()
        {
            sut.Recreate = false;

            SetupCreated(2);
            SetupDeleted();

            A.CallTo(() => persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>._, default))
                .Throws(new InconsistentStateException(2, -1)).Once();

            await Assert.ThrowsAsync<DomainObjectConflictException>(() => sut.ExecuteAsync(new CreateAuto { Value = 4 }));
        }

        [Fact]
        public async Task Should_recreate_with_upsert_command_if_deleted_before()
        {
            sut.Recreate = true;
            sut.RecreateEvent = true;

            SetupCreated(2);
            SetupDeleted();

            A.CallTo(() => persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>._, default))
                .Throws(new InconsistentStateException(2, -1)).Once();

            var result = await sut.ExecuteAsync(new Upsert { Value = 4 });

            A.CallTo(() => persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>.That.Matches(x => x.Count == 1), default))
                .MustHaveHappenedANumberOfTimesMatching(x => x == 3);
            A.CallTo(() => persistence.ReadAsync(A<long>._, default))
                .MustHaveHappened();

            Assert.Equal(CommandResult.Empty(id, 2, 1), result);
            Assert.Equal(2, sut.Version);
            Assert.Equal(2, sut.Snapshot.Version);

            Assert.Empty(sut.GetUncomittedEvents());
            AssertSnapshot(sut.Snapshot, 4, 2);
        }

        [Fact]
        public async Task Should_throw_exception_if_recreation_with_upsert_command_not_allowed()
        {
            sut.Recreate = false;

            SetupCreated(2);
            SetupDeleted();

            A.CallTo(() => persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>._, default))
                .Throws(new InconsistentStateException(2, -1)).Once();

            await Assert.ThrowsAsync<DomainObjectDeletedException>(() => sut.ExecuteAsync(new Upsert { Value = 4 }));
        }

        [Fact]
        public async Task Should_write_state_and_events_if_updated_after_creation()
        {
            SetupEmpty();

            await sut.ExecuteAsync(new CreateAuto { Value = 4 });

            var result = await sut.ExecuteAsync(new UpdateAuto { Value = 8, ExpectedVersion = 0 });

            A.CallTo(() => persistence.WriteSnapshotAsync(A<MyDomainState>.That.Matches(x => x.Value == 8), default))
                .MustHaveHappened();
            A.CallTo(() => persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>.That.Matches(x => x.Count == 1), default))
                .MustHaveHappened();
            A.CallTo(() => persistence.ReadAsync(A<long>._, default))
                .MustNotHaveHappened();

            Assert.Equal(CommandResult.Empty(id, 1, 0), result);
            Assert.Equal(1, sut.Version);
            Assert.Equal(1, sut.Snapshot.Version);

            Assert.Empty(sut.GetUncomittedEvents());
            AssertSnapshot(sut.Snapshot, 8, 1);
        }

        [Fact]
        public async Task Should_write_state_and_events_if_updated()
        {
            SetupCreated(4);

            var result = await sut.ExecuteAsync(new UpdateAuto { Value = 8, ExpectedVersion = 0 });

            A.CallTo(() => persistence.WriteSnapshotAsync(A<MyDomainState>.That.Matches(x => x.Value == 8), default))
                .MustHaveHappened();
            A.CallTo(() => persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>.That.Matches(x => x.Count == 1), default))
                .MustHaveHappened();
            A.CallTo(() => persistence.ReadAsync(A<long>._, default))
                .MustHaveHappenedOnceExactly();

            Assert.Equal(CommandResult.Empty(id, 1, 0), result);
            Assert.Equal(1, sut.Version);
            Assert.Equal(1, sut.Snapshot.Version);

            Assert.Empty(sut.GetUncomittedEvents());
            AssertSnapshot(sut.Snapshot, 8, 1);
        }

        [Fact]
        public async Task Should_not_load_on_create()
        {
            SetupEmpty();

            await sut.ExecuteAsync(new CreateAuto());

            A.CallTo(() => persistence.ReadAsync(A<long>._, default))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_load_once_on_update()
        {
            SetupCreated(4);

            await sut.ExecuteAsync(new UpdateAuto { Value = 8, ExpectedVersion = 0 });
            await sut.ExecuteAsync(new UpdateAuto { Value = 9, ExpectedVersion = 1 });

            A.CallTo(() => persistence.ReadAsync(A<long>._, default))
                .MustHaveHappenedOnceExactly();

            Assert.Empty(sut.GetUncomittedEvents());
            AssertSnapshot(sut.Snapshot, 9, 2);
        }

        [Fact]
        public async Task Should_rebuild_state()
        {
            SetupCreated(4);

            await sut.RebuildStateAsync();

            A.CallTo(() => persistence.WriteSnapshotAsync(A<MyDomainState>.That.Matches(x => x.Value == 4), default))
                .MustHaveHappened();
            A.CallTo(() => persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>._, default))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_throw_on_rebuild_if_no_event_found()
        {
            SetupEmpty();

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.RebuildStateAsync());
        }

        [Fact]
        public async Task Should_throw_exception_on_create_command_is_rejected_due_to_version_conflict()
        {
            SetupEmpty();

            A.CallTo(() => persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>._, default))
                .Throws(new InconsistentStateException(4, EtagVersion.Empty));

            await Assert.ThrowsAsync<DomainObjectConflictException>(() => sut.ExecuteAsync(new CreateAuto()));
        }

        [Fact]
        public async Task Should_throw_exception_if_create_command_is_invoked_for_loaded_and_created_object()
        {
            await sut.ExecuteAsync(new CreateAuto());

            await Assert.ThrowsAsync<DomainObjectConflictException>(() => sut.ExecuteAsync(new CreateAuto()));
        }

        [Fact]
        public async Task Should_throw_exception_if_create_command_not_accepted()
        {
            SetupEmpty();

            await Assert.ThrowsAsync<DomainException>(() => sut.ExecuteAsync(new CreateAuto { Value = 99 }));
        }

        [Fact]
        public async Task Should_return_custom_result_on_create()
        {
            SetupEmpty();

            var result = await sut.ExecuteAsync(new CreateCustom());

            Assert.Equal(new CommandResult(id, 0, EtagVersion.Empty, "CREATED"), result);
        }

        [Fact]
        public async Task Should_throw_exception_if_update_command_invoked_for_empty_object()
        {
            SetupEmpty();

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.ExecuteAsync(new UpdateAuto()));
        }

        [Fact]
        public async Task Should_throw_exception_if_update_command_not_accepted()
        {
            SetupCreated(4);

            await Assert.ThrowsAsync<DomainException>(() => sut.ExecuteAsync(new UpdateAuto { Value = 99 }));
        }

        [Fact]
        public async Task Should_return_custom_result_on_update()
        {
            SetupCreated(4);

            var result = await sut.ExecuteAsync(new UpdateCustom());

            Assert.Equal(new CommandResult(id, 1, 0, "UPDATED"), result);
        }

        [Fact]
        public async Task Should_throw_exception_if_other_verison_expected()
        {
            SetupCreated(4);

            await Assert.ThrowsAsync<DomainObjectVersionException>(() => sut.ExecuteAsync(new UpdateCustom { ExpectedVersion = 3 }));
        }

        [Fact]
        public async Task Should_not_update_if_snapshot_is_not_changed()
        {
            SetupCreated(4);

            var result = await sut.ExecuteAsync(new UpdateAuto { Value = MyDomainState.Unchanged });

            Assert.Equal(CommandResult.Empty(id, 0, 0), result);
            Assert.Equal(0, sut.Version);
            Assert.Equal(0, sut.Snapshot.Version);

            Assert.Empty(sut.GetUncomittedEvents());
            AssertSnapshot(sut.Snapshot, 4, 0);
        }

        [Fact]
        public async Task Should_reset_state_if_writing_snapshot_for_create_failed()
        {
            SetupEmpty();

            A.CallTo(() => persistence.WriteSnapshotAsync(A<MyDomainState>._, default))
                .Throws(new InvalidOperationException());

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ExecuteAsync(new CreateAuto()));

            Assert.Empty(sut.GetUncomittedEvents());
            AssertSnapshot(sut.Snapshot, 0, EtagVersion.Empty);
        }

        [Fact]
        public async Task Should_reset_state_if_writing_snapshot_for_update_failed()
        {
            SetupCreated(4);

            A.CallTo(() => persistence.WriteSnapshotAsync(A<MyDomainState>._, default))
                .Throws(new InvalidOperationException());

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ExecuteAsync(new UpdateAuto()));

            Assert.Empty(sut.GetUncomittedEvents());
            AssertSnapshot(sut.Snapshot, 4, 0);
        }

        [Fact]
        public async Task Should_write_events_to_delete_stream_on_delete()
        {
            SetupCreated(4);
            SetupDeleted();

            var deleteStream = A.Fake<IPersistence<MyDomainState>>();

            A.CallTo(() => persistenceFactory.WithEventSourcing(typeof(MyDomainObject), DomainId.Combine(id, DomainId.Create("deleted")), null))
                .Returns(deleteStream);

            await sut.ExecuteAsync(new DeletePermanent());

            AssertSnapshot(sut.Snapshot, 0, EtagVersion.Empty, false);

            A.CallTo(() => persistence.DeleteAsync(default))
                .MustHaveHappened();

            A.CallTo(() => persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>._, default))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => deleteStream.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>._, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_get_old_versions_if_cached()
        {
            sut.VersionsToKeep = int.MaxValue;

            SetupEmpty();

            await sut.ExecuteAsync(new CreateAuto { Value = 3 });
            await sut.ExecuteAsync(new UpdateAuto { Value = 4 });
            await sut.ExecuteAsync(new UpdateAuto { Value = 5 });

            var version_Empty = await sut.GetSnapshotAsync(EtagVersion.Empty);
            var version_0 = await sut.GetSnapshotAsync(0);
            var version_1 = await sut.GetSnapshotAsync(1);
            var version_2 = await sut.GetSnapshotAsync(2);

            Assert.Empty(sut.GetUncomittedEvents());
            AssertSnapshot(version_Empty, 0, EtagVersion.Empty);
            AssertSnapshot(version_0, 3, 0);
            AssertSnapshot(version_1, 4, 1);
            AssertSnapshot(version_2, 5, 2);

            A.CallTo(() => persistenceFactory.WithEventSourcing(typeof(MyDomainObject), id, A<HandleEvent>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_get_old_versions_from_query_if_not_cached()
        {
            sut.VersionsToKeep = 1;

            SetupEmpty();
            SetupLoaded();

            await sut.ExecuteAsync(new CreateAuto { Value = 3 });
            await sut.ExecuteAsync(new UpdateAuto { Value = 4 });
            await sut.ExecuteAsync(new UpdateAuto { Value = 5 });

            var version_Empty = await sut.GetSnapshotAsync(EtagVersion.Empty);
            var version_0 = await sut.GetSnapshotAsync(0);
            var version_1 = await sut.GetSnapshotAsync(1);
            var version_2 = await sut.GetSnapshotAsync(2);

            Assert.Empty(sut.GetUncomittedEvents());
            AssertSnapshot(version_Empty, 0, EtagVersion.Empty);
            AssertSnapshot(version_0, 3, 0);
            AssertSnapshot(version_1, 4, 1);
            AssertSnapshot(version_2, 5, 2);

            A.CallTo(() => persistenceFactory.WithEventSourcing(typeof(MyDomainObject), id, A<HandleEvent>._))
                .MustHaveHappenedOnceExactly();
        }

        private static void AssertSnapshot(MyDomainState state, int value, long version, bool isDeleted = false)
        {
            Assert.Equal(new MyDomainState { Value = value, Version = version, IsDeleted = isDeleted }, state);
        }

        private void SetupDeleted()
        {
            sut.ExecuteAsync(new Delete()).Wait();
        }

        private void SetupCreated(int value)
        {
            SetupCreated(new ValueChanged { Value = value });
        }

        private void SetupCreated(params IEvent[] @events)
        {
            var version = -1;

            A.CallTo(() => persistence.ReadAsync(-2, default))
                .Invokes(() =>
                {
                    version++;

                    foreach (var @event in events)
                    {
                        handleEvent?.Invoke(Envelope.Create(@event));
                    }
                });

            A.CallTo(() => persistence.Version)
                .ReturnsLazily(() => version);
        }

        private void SetupLoaded()
        {
            var @events = new List<Envelope<IEvent>>();

            A.CallTo(() => persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>._, default))
                .Invokes(args =>
                {
                    @events.AddRange(args.GetArgument<IReadOnlyList<Envelope<IEvent>>>(0)!);
                });

            A.CallTo(() => persistenceEvents.ReadAsync(EtagVersion.Any, default))
                .Invokes(_ =>
                {
                    foreach (var @event in events)
                    {
                        handleEvent?.Invoke(@event);
                    }
                });
        }

        private void SetupEmpty()
        {
            A.CallTo(() => persistenceFactory.WithSnapshotsAndEventSourcing(typeof(MyDomainObject), id, A<HandleSnapshot<MyDomainState>>._, A<HandleEvent>._))
                .Returns(persistence);

            A.CallTo(() => persistence.Version)
                .Returns(-1);
        }
    }
}
