// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Infrastructure.States;

public class SimpleStateTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly TestState<MyDomainState> testState = new TestState<MyDomainState>(DomainId.NewGuid());
    private readonly SimpleState<MyDomainState> sut;

    public SimpleStateTests()
    {
        ct = cts.Token;

        sut = new SimpleState<MyDomainState>(testState.PersistenceFactory, GetType(), testState.Id);
    }

    [Fact]
    public void Should_init_with_base_data()
    {
        Assert.Equal(-1, sut.Version);
        Assert.NotNull(sut.Value);
    }

    [Fact]
    public async Task Should_get_state_from_persistence_on_load()
    {
        testState.Version = 42;
        testState.Snapshot = new MyDomainState { Value = 50 };

        await sut.LoadAsync(ct);

        Assert.Equal(42, sut.Version);
        Assert.Equal(50, sut.Value.Value);

        A.CallTo(() => testState.Persistence.ReadAsync(-2, ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_delete_when_clearing()
    {
        await sut.ClearAsync(ct);

        A.CallTo(() => testState.Persistence.DeleteAsync(ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_invoke_persistence_when_writing_state()
    {
        await sut.WriteAsync(ct);

        A.CallTo(() => testState.Persistence.WriteSnapshotAsync(sut.Value, ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_load_once_on_update()
    {
        await sut.UpdateAsync(x => true, ct: ct);
        await sut.UpdateAsync(x => true, ct: ct);

        A.CallTo(() => testState.Persistence.ReadAsync(-2, ct))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_write_state_on_update_when_callback_returns_true()
    {
        await sut.UpdateAsync(x => true, ct: ct);

        A.CallTo(() => testState.Persistence.WriteSnapshotAsync(sut.Value, ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_write_state_on_update_when_callback_returns_false()
    {
        await sut.UpdateAsync(x => true, ct: ct);

        A.CallTo(() => testState.Persistence.WriteSnapshotAsync(sut.Value, ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_write_state_on_update_and_return_when_callback_returns_true()
    {
        var actual = await sut.UpdateAsync(x => (true, 42), ct: ct);

        Assert.Equal(42, actual);

        A.CallTo(() => testState.Persistence.WriteSnapshotAsync(sut.Value, ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_write_state_on_update_and_return_when_callback_returns_false()
    {
        var actual = await sut.UpdateAsync(x => (false, 42), ct: ct);

        Assert.Equal(42, actual);

        A.CallTo(() => testState.Persistence.WriteSnapshotAsync(sut.Value, ct))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_retry_update_when_failed_with_inconsistency_issue()
    {
        A.CallTo(() => testState.Persistence.WriteSnapshotAsync(sut.Value, ct))
            .Throws(new InconsistentStateException(1, 2)).NumberOfTimes(5);

        await sut.UpdateAsync(x => true, ct: ct);

        A.CallTo(() => testState.Persistence.WriteSnapshotAsync(sut.Value, ct))
            .MustHaveHappenedANumberOfTimesMatching(x => x == 6);

        A.CallTo(() => testState.Persistence.ReadAsync(A<long>._, ct))
            .MustHaveHappenedANumberOfTimesMatching(x => x == 6);
    }

    [Fact]
    public async Task Should_give_up_update_after_too_many_inconsistency_issues()
    {
        A.CallTo(() => testState.Persistence.WriteSnapshotAsync(sut.Value, ct))
            .Throws(new InconsistentStateException(1, 2)).NumberOfTimes(100);

        await Assert.ThrowsAsync<InconsistentStateException>(() => sut.UpdateAsync(x => true, ct: ct));

        A.CallTo(() => testState.Persistence.WriteSnapshotAsync(sut.Value, ct))
            .MustHaveHappenedANumberOfTimesMatching(x => x == 20);

        A.CallTo(() => testState.Persistence.ReadAsync(A<long>._, ct))
            .MustHaveHappenedANumberOfTimesMatching(x => x == 20);
    }

    [Fact]
    public async Task Should_not_retry_update_with_other_exception()
    {
        A.CallTo(() => testState.Persistence.WriteSnapshotAsync(sut.Value, ct))
            .Throws(new InvalidOperationException());

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.UpdateAsync(x => true, ct: ct));

        A.CallTo(() => testState.Persistence.WriteSnapshotAsync(sut.Value, ct))
            .MustHaveHappenedANumberOfTimesMatching(x => x == 1);

        A.CallTo(() => testState.Persistence.ReadAsync(A<long>._, ct))
            .MustHaveHappenedANumberOfTimesMatching(x => x == 1);
    }

    [Fact]
    public async Task Should_retry_update_and_return_when_failed_with_inconsistency_issue()
    {
        A.CallTo(() => testState.Persistence.WriteSnapshotAsync(sut.Value, ct))
            .Throws(new InconsistentStateException(1, 2)).NumberOfTimes(5);

        await sut.UpdateAsync(x => (true, 42), ct: ct);

        A.CallTo(() => testState.Persistence.WriteSnapshotAsync(sut.Value, ct))
            .MustHaveHappenedANumberOfTimesMatching(x => x == 6);

        A.CallTo(() => testState.Persistence.ReadAsync(A<long>._, ct))
            .MustHaveHappenedANumberOfTimesMatching(x => x == 6);
    }

    [Fact]
    public async Task Should_give_up_update_and_return_after_too_many_inconsistency_issues()
    {
        A.CallTo(() => testState.Persistence.WriteSnapshotAsync(sut.Value, ct))
            .Throws(new InconsistentStateException(1, 2)).NumberOfTimes(100);

        await Assert.ThrowsAsync<InconsistentStateException>(() => sut.UpdateAsync(x => (true, 42), ct: ct));

        A.CallTo(() => testState.Persistence.WriteSnapshotAsync(sut.Value, ct))
            .MustHaveHappenedANumberOfTimesMatching(x => x == 20);

        A.CallTo(() => testState.Persistence.ReadAsync(A<long>._, ct))
            .MustHaveHappenedANumberOfTimesMatching(x => x == 20);
    }

    [Fact]
    public async Task Should_not_retry_update_and_return_with_other_exception()
    {
        A.CallTo(() => testState.Persistence.WriteSnapshotAsync(sut.Value, ct))
            .Throws(new InvalidOperationException());

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.UpdateAsync(x => (true, 42), ct: ct));

        A.CallTo(() => testState.Persistence.WriteSnapshotAsync(sut.Value, ct))
            .MustHaveHappenedANumberOfTimesMatching(x => x == 1);

        A.CallTo(() => testState.Persistence.ReadAsync(A<long>._, ct))
            .MustHaveHappenedANumberOfTimesMatching(x => x == 1);
    }

    [Fact]
    public async Task Should_not_written_if_period_not_over()
    {
        var now = SystemClock.Instance.GetCurrentInstant();

        var clock = A.Fake<IClock>();

        A.CallTo(() => clock.GetCurrentInstant())
            .Returns(now).NumberOfTimes(5).Then
            .Returns(now.Plus(Duration.FromSeconds(2)));

        sut.Clock = clock;

        for (var i = 0; i < 10; i++)
        {
            await sut.WriteAsync(1000, ct);
        }

        await sut.UpdateAsync(x => true, ct: ct);

        A.CallTo(() => testState.Persistence.WriteSnapshotAsync(sut.Value, ct))
            .MustHaveHappenedANumberOfTimesMatching(x => x == 3);
    }
}
