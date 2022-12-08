// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Infrastructure.Commands;

public class DomainObjectTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly TestState<MyDomainState> state;
    private readonly DomainId id = DomainId.NewGuid();
    private readonly MyDomainObject sut;

    public DomainObjectTests()
    {
        ct = cts.Token;

        state = new TestState<MyDomainState>(id);

        sut = new MyDomainObject(id, state.PersistenceFactory);
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
        A.CallTo(() => state.Persistence.IsSnapshotStale)
            .Returns(true);

        SetupCreated(1);

        await sut.EnsureLoadedAsync(ct);

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<MyDomainState>._, default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_repair_if_not_stale()
    {
        A.CallTo(() => state.Persistence.IsSnapshotStale)
            .Returns(false);

        SetupCreated(1);

        await sut.EnsureLoadedAsync(ct);

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<MyDomainState>._, default))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_write_state_and_events_if_created()
    {
        var actual = await sut.ExecuteAsync(new CreateAuto { Value = 4 }, ct);

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<MyDomainState>.That.Matches(x => x.Value == 4), default))
            .MustHaveHappened();

        A.CallTo(() => state.Persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>.That.Matches(x => x.Count == 1), ct))
            .MustHaveHappened();

        A.CallTo(() => state.Persistence.ReadAsync(A<long>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        Assert.Equal(CommandResult.Empty(id, 0, EtagVersion.Empty), actual);
        Assert.Equal(0, sut.Version);
        Assert.Equal(0, sut.Snapshot.Version);

        Assert.Empty(sut.GetUncomittedEvents());
        AssertSnapshot(sut.Snapshot, 4, 0);
    }

    [Fact]
    public async Task Should_migrate_old_event_with_state()
    {
        SetupCreated(new ValueChanged { Value = 10 }, new MultipleByTwiceEvent());

        await sut.EnsureLoadedAsync(ct);

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

        await sut.EnsureLoadedAsync(ct);

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

        await sut.EnsureLoadedAsync(ct);

        Assert.Equal(2, sut.Version);
        Assert.Equal(2, sut.Snapshot.Version);

        AssertSnapshot(sut.Snapshot, 3, 2, true);
    }

    [Fact]
    public async Task Should_throw_exception_if_writing_causes_inconsistent_state_exception()
    {
        sut.Recreate = false;

        SetupCreated(2);

        A.CallTo(() => state.Persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>._, ct))
            .Throws(new InconsistentStateException(2, -1)).Once();

        await Assert.ThrowsAsync<DomainObjectVersionException>(() => sut.ExecuteAsync(new UpdateAuto(), ct));
    }

    [Fact]
    public async Task Should_throw_exception_if_writing_causes_inconsistent_state_exception_and_deleted()
    {
        sut.Recreate = false;

        SetupCreated(2);
        SetupDeleted();

        A.CallTo(() => state.Persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>._, ct))
            .Throws(new InconsistentStateException(2, -1)).Once();

        await Assert.ThrowsAsync<DomainObjectDeletedException>(() => sut.ExecuteAsync(new UpdateAuto(), ct));
    }

    [Fact]
    public async Task Should_recreate_with_create_command_if_deleted_before()
    {
        sut.Recreate = true;
        sut.RecreateEvent = true;

        SetupCreated(2);
        SetupDeleted();

        A.CallTo(() => state.Persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>._, ct))
            .Throws(new InconsistentStateException(2, -1)).Once();

        var actual = await sut.ExecuteAsync(new CreateAuto { Value = 4 }, ct);

        A.CallTo(() => state.Persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>.That.Matches(x => x.Count == 1), ct))
            .MustHaveHappenedANumberOfTimesMatching(x => x == 3);

        A.CallTo(() => state.Persistence.ReadAsync(A<long>._, ct))
            .MustHaveHappened();

        Assert.Equal(CommandResult.Empty(id, 2, 1), actual);
        Assert.Equal(2, sut.Version);
        Assert.Equal(2, sut.Snapshot.Version);

        Assert.Empty(sut.GetUncomittedEvents());
        AssertSnapshot(sut.Snapshot, 4, 2);
    }

    [Fact]
    public async Task Should_throw_exception_if_recreation_with_create_command_is_not_allowed()
    {
        sut.Recreate = false;

        SetupCreated(2);
        SetupDeleted();

        A.CallTo(() => state.Persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>._, ct))
            .Throws(new InconsistentStateException(2, -1)).Once();

        await Assert.ThrowsAsync<DomainObjectConflictException>(() => sut.ExecuteAsync(new CreateAuto(), ct));
    }

    [Fact]
    public async Task Should_recreate_with_upsert_command_if_deleted_before()
    {
        sut.Recreate = true;
        sut.RecreateEvent = true;

        SetupCreated(2);
        SetupDeleted();

        A.CallTo(() => state.Persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>._, ct))
            .Throws(new InconsistentStateException(2, -1)).Once();

        var actual = await sut.ExecuteAsync(new Upsert { Value = 4 }, ct);

        A.CallTo(() => state.Persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>.That.Matches(x => x.Count == 1), ct))
            .MustHaveHappenedANumberOfTimesMatching(x => x == 3);

        A.CallTo(() => state.Persistence.ReadAsync(A<long>._, ct))
            .MustHaveHappened();

        Assert.Equal(CommandResult.Empty(id, 2, 1), actual);
        Assert.Equal(2, sut.Version);
        Assert.Equal(2, sut.Snapshot.Version);

        Assert.Empty(sut.GetUncomittedEvents());
        AssertSnapshot(sut.Snapshot, 4, 2);
    }

    [Fact]
    public async Task Should_throw_exception_if_recreation_with_upsert_command_is_not_allowed()
    {
        sut.Recreate = false;

        SetupCreated(2);
        SetupDeleted();

        A.CallTo(() => state.Persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>._, ct))
            .Throws(new InconsistentStateException(2, -1)).Once();

        await Assert.ThrowsAsync<DomainObjectDeletedException>(() => sut.ExecuteAsync(new Upsert(), ct));
    }

    [Fact]
    public async Task Should_write_state_and_events_if_updated_after_creation()
    {
        await sut.ExecuteAsync(new CreateAuto { Value = 4 }, ct);

        var actual = await sut.ExecuteAsync(new UpdateAuto { Value = 8, ExpectedVersion = 0 }, ct);

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<MyDomainState>.That.Matches(x => x.Value == 8), default))
            .MustHaveHappened();

        A.CallTo(() => state.Persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>.That.Matches(x => x.Count == 1), ct))
            .MustHaveHappened();

        A.CallTo(() => state.Persistence.ReadAsync(A<long>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        Assert.Equal(CommandResult.Empty(id, 1, 0), actual);
        Assert.Equal(1, sut.Version);
        Assert.Equal(1, sut.Snapshot.Version);

        Assert.Empty(sut.GetUncomittedEvents());
        AssertSnapshot(sut.Snapshot, 8, 1);
    }

    [Fact]
    public async Task Should_write_state_and_events_if_updated()
    {
        SetupCreated(4);

        var actual = await sut.ExecuteAsync(new UpdateAuto { Value = 8, ExpectedVersion = 0 }, ct);

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<MyDomainState>.That.Matches(x => x.Value == 8), default))
            .MustHaveHappened();

        A.CallTo(() => state.Persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>.That.Matches(x => x.Count == 1), ct))
            .MustHaveHappened();

        A.CallTo(() => state.Persistence.ReadAsync(A<long>._, ct))
            .MustHaveHappenedOnceExactly();

        Assert.Equal(CommandResult.Empty(id, 1, 0), actual);
        Assert.Equal(1, sut.Version);
        Assert.Equal(1, sut.Snapshot.Version);

        Assert.Empty(sut.GetUncomittedEvents());
        AssertSnapshot(sut.Snapshot, 8, 1);
    }

    [Fact]
    public async Task Should_not_load_on_create()
    {
        await sut.ExecuteAsync(new CreateAuto(), ct);

        A.CallTo(() => state.Persistence.ReadAsync(A<long>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_load_once_on_update()
    {
        SetupCreated(4);

        await sut.ExecuteAsync(new UpdateAuto { Value = 8, ExpectedVersion = 0 }, ct);
        await sut.ExecuteAsync(new UpdateAuto { Value = 9, ExpectedVersion = 1 }, ct);

        A.CallTo(() => state.Persistence.ReadAsync(A<long>._, ct))
            .MustHaveHappenedOnceExactly();

        Assert.Empty(sut.GetUncomittedEvents());
        AssertSnapshot(sut.Snapshot, 9, 2);
    }

    [Fact]
    public async Task Should_rebuild_state()
    {
        SetupCreated(4);

        await sut.RebuildStateAsync(ct);

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<MyDomainState>.That.Matches(x => x.Value == 4), ct))
            .MustHaveHappened();

        A.CallTo(() => state.Persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_throw_on_rebuild_if_no_event_found()
    {
        await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.RebuildStateAsync(ct));
    }

    [Fact]
    public async Task Should_throw_exception_on_create_command_is_rejected_due_to_version_conflict()
    {
        A.CallTo(() => state.Persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>._, ct))
            .Throws(new InconsistentStateException(4, EtagVersion.Empty));

        await Assert.ThrowsAsync<DomainObjectConflictException>(() => sut.ExecuteAsync(new CreateAuto(), ct));
    }

    [Fact]
    public async Task Should_throw_exception_if_create_command_is_invoked_for_loaded_and_created_object()
    {
        await sut.ExecuteAsync(new CreateAuto(), ct);

        await Assert.ThrowsAsync<DomainObjectConflictException>(() => sut.ExecuteAsync(new CreateAuto(), ct));
    }

    [Fact]
    public async Task Should_throw_exception_if_create_command_not_accepted()
    {
        await Assert.ThrowsAsync<DomainException>(() => sut.ExecuteAsync(new CreateAuto { Value = 99 }, ct));
    }

    [Fact]
    public async Task Should_return_custom_actual_on_create()
    {
        var actual = await sut.ExecuteAsync(new CreateCustom(), ct);

        Assert.Equal(new CommandResult(id, 0, EtagVersion.Empty, "CREATED"), actual);
    }

    [Fact]
    public async Task Should_throw_exception_if_update_command_invoked_for_empty_object()
    {
        await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.ExecuteAsync(new UpdateAuto(), ct));
    }

    [Fact]
    public async Task Should_throw_exception_if_update_command_not_accepted()
    {
        SetupCreated(4);

        await Assert.ThrowsAsync<DomainException>(() => sut.ExecuteAsync(new UpdateAuto { Value = 99 }, ct));
    }

    [Fact]
    public async Task Should_return_custom_actual_on_update()
    {
        SetupCreated(4);

        var actual = await sut.ExecuteAsync(new UpdateCustom(), ct);

        Assert.Equal(new CommandResult(id, 1, 0, "UPDATED"), actual);
    }

    [Fact]
    public async Task Should_throw_exception_if_other_verison_expected()
    {
        SetupCreated(4);

        await Assert.ThrowsAsync<DomainObjectVersionException>(() => sut.ExecuteAsync(new UpdateCustom { ExpectedVersion = 3 }, ct));
    }

    [Fact]
    public async Task Should_not_update_if_snapshot_is_not_changed()
    {
        SetupCreated(4);

        var actual = await sut.ExecuteAsync(new UpdateAuto { Value = MyDomainState.Unchanged }, ct);

        Assert.Equal(CommandResult.Empty(id, 0, 0), actual);
        Assert.Equal(0, sut.Version);
        Assert.Equal(0, sut.Snapshot.Version);

        Assert.Empty(sut.GetUncomittedEvents());
        AssertSnapshot(sut.Snapshot, 4, 0);
    }

    [Fact]
    public async Task Should_reset_state_if_writing_snapshot_for_create_failed()
    {
        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<MyDomainState>._, default))
            .Throws(new InvalidOperationException());

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ExecuteAsync(new CreateAuto(), ct));

        Assert.Empty(sut.GetUncomittedEvents());
        AssertSnapshot(sut.Snapshot, 0, EtagVersion.Empty);
    }

    [Fact]
    public async Task Should_reset_state_if_writing_snapshot_for_update_failed()
    {
        SetupCreated(4);

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<MyDomainState>._, default))
            .Throws(new InvalidOperationException());

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ExecuteAsync(new UpdateAuto(), ct));

        Assert.Empty(sut.GetUncomittedEvents());
        AssertSnapshot(sut.Snapshot, 4, 0);
    }

    [Fact]
    public async Task Should_write_events_to_delete_stream_on_delete()
    {
        SetupCreated(4);
        SetupDeleted();

        var deleteStream = A.Fake<IPersistence<MyDomainState>>();

        A.CallTo(() => state.PersistenceFactory.WithEventSourcing(typeof(MyDomainObject), DomainId.Combine(id, DomainId.Create("deleted")), null))
            .Returns(deleteStream);

        await sut.ExecuteAsync(new DeletePermanent(), ct);

        AssertSnapshot(sut.Snapshot, 0, EtagVersion.Empty, false);

        A.CallTo(() => state.Persistence.DeleteAsync(ct))
            .MustHaveHappened();

        A.CallTo(() => state.Persistence.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>._, ct))
            .MustHaveHappenedOnceExactly();

        A.CallTo(() => deleteStream.WriteEventsAsync(A<IReadOnlyList<Envelope<IEvent>>>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_get_old_versions_from_query()
    {
        await sut.ExecuteAsync(new CreateAuto { Value = 3 }, ct);
        await sut.ExecuteAsync(new UpdateAuto { Value = 4 }, ct);
        await sut.ExecuteAsync(new UpdateAuto { Value = 5 }, ct);

        var version_Empty = await sut.GetSnapshotAsync(EtagVersion.Empty, ct);
        var version_0 = await sut.GetSnapshotAsync(0, ct);
        var version_1 = await sut.GetSnapshotAsync(1, ct);
        var version_2 = await sut.GetSnapshotAsync(2, ct);

        Assert.Empty(sut.GetUncomittedEvents());
        AssertSnapshot(version_Empty, 0, EtagVersion.Empty);
        AssertSnapshot(version_0, 3, 0);
        AssertSnapshot(version_1, 4, 1);
        AssertSnapshot(version_2, 5, 2);

        A.CallTo(() => state.PersistenceFactory.WithEventSourcing(typeof(MyDomainObject), id, A<HandleEvent>._))
            .MustHaveHappened(3, Times.Exactly);
    }

    private static void AssertSnapshot(MyDomainState state, int value, long version, bool isDeleted = false)
    {
        Assert.Equal(new MyDomainState { Value = value, Version = version, IsDeleted = isDeleted }, state);
    }

    private void SetupDeleted()
    {
        sut.ExecuteAsync(new Delete(), ct).Wait(ct);
    }

    private void SetupCreated(int value)
    {
        state.AddEvent(new ValueChanged { Value = value });
    }

    private void SetupCreated(params IEvent[] events)
    {
        events.Foreach(@event => state.AddEvent(@event));
    }
}
