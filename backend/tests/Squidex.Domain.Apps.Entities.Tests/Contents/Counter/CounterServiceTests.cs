// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Domain.Apps.Entities.Contents.Counter;

public class CounterServiceTests : GivenContext
{
    private readonly TestState<CounterService.State> state;
    private readonly CounterService sut;

    public CounterServiceTests()
    {
        state = new TestState<CounterService.State>(AppId.Id);

        sut = new CounterService(state.PersistenceFactory);
    }

    [Fact]
    public void Should_run_delete_with_default_order()
    {
        var order = ((IDeleter)sut).Order;

        Assert.Equal(0, order);
    }

    [Fact]
    public async Task Should_delete_state_when_app_deleted()
    {
        await ((IDeleter)sut).DeleteAppAsync(App, CancellationToken);

        A.CallTo(() => state.Persistence.DeleteAsync(CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_increment_counters()
    {
        Assert.Equal(1, await sut.IncrementAsync(AppId.Id, "Counter1", CancellationToken));
        Assert.Equal(2, await sut.IncrementAsync(AppId.Id, "Counter1", CancellationToken));

        Assert.Equal(1, await sut.IncrementAsync(AppId.Id, "Counter2", CancellationToken));
        Assert.Equal(2, await sut.IncrementAsync(AppId.Id, "Counter2", CancellationToken));

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<CounterService.State>._, CancellationToken))
            .MustHaveHappened(4, Times.Exactly);
    }

    [Fact]
    public async Task Should_reset_counter()
    {
        Assert.Equal(1, await sut.IncrementAsync(AppId.Id, "Counter1", CancellationToken));
        Assert.Equal(2, await sut.IncrementAsync(AppId.Id, "Counter1", CancellationToken));

        Assert.Equal(1, await sut.ResetAsync(AppId.Id, "Counter1", 1, CancellationToken));

        Assert.Equal(2, await sut.IncrementAsync(AppId.Id, "Counter1", CancellationToken));

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<CounterService.State>._, CancellationToken))
            .MustHaveHappened(4, Times.Exactly);
    }
}
