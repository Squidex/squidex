// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Domain.Apps.Entities.Apps;

public sealed class AppUISettingsTests : GivenContext
{
    private readonly TestState<AppUISettings.State> state;
    private readonly string userId = Guid.NewGuid().ToString();
    private readonly string stateId;
    private readonly AppUISettings sut;

    public AppUISettingsTests()
    {
        stateId = $"{AppId.Id}_{userId}";
        state = new TestState<AppUISettings.State>(stateId);

        sut = new AppUISettings(state.PersistenceFactory);
    }

    [Fact]
    public void Should_run_with_default_order()
    {
        var order = ((IDeleter)sut).Order;

        Assert.Equal(0, order);
    }

    [Fact]
    public async Task Should_delete_contributor_state()
    {
        await ((IDeleter)sut).DeleteContributorAsync(AppId.Id, userId, CancellationToken);

        A.CallTo(() => state.Persistence.DeleteAsync(CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_delete_app_and_contributors()
    {
        App = App with
        {
            Contributors = Contributors.Empty.Assign(userId, Role.Owner)
        };

        var rootState = new TestState<AppUISettings.State>(AppId.Id, state.PersistenceFactory);

        await ((IDeleter)sut).DeleteAppAsync(App, CancellationToken);

        A.CallTo(() => state.Persistence.DeleteAsync(CancellationToken))
            .MustHaveHappened();

        A.CallTo(() => rootState.Persistence.DeleteAsync(CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_set_setting()
    {
        await sut.SetAsync(AppId.Id, userId, JsonValue.Object().Add("key", 42), CancellationToken);

        var actual = await sut.GetAsync(AppId.Id, userId, CancellationToken);

        var expected =
            JsonValue.Object().Add("key", 42);

        Assert.Equal(expected.ToString(), actual.ToString());

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<AppUISettings.State>._, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_set_root_value()
    {
        await sut.SetAsync(AppId.Id, userId, "key", 42, CancellationToken);

        var actual = await sut.GetAsync(AppId.Id, userId, CancellationToken);

        var expected =
            JsonValue.Object().Add("key", 42);

        Assert.Equal(expected.ToString(), actual.ToString());

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<AppUISettings.State>._, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_remove_root_value()
    {
        await sut.SetAsync(AppId.Id, userId, "key", 42, CancellationToken);

        await sut.RemoveAsync(AppId.Id, userId, "key", CancellationToken);
        await sut.RemoveAsync(AppId.Id, userId, "key", CancellationToken);

        var actual = await sut.GetAsync(AppId.Id, userId, CancellationToken);

        var expected = JsonValue.Object();

        Assert.Equal(expected.ToString(), actual.ToString());

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<AppUISettings.State>._, CancellationToken))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_set_nested_value()
    {
        await sut.SetAsync(AppId.Id, userId, "root.nested", 42, CancellationToken);

        var actual = await sut.GetAsync(AppId.Id, userId, CancellationToken);

        var expected =
            JsonValue.Object().Add("root",
                JsonValue.Object().Add("nested", 42));

        Assert.Equal(expected.ToString(), actual.ToString());

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<AppUISettings.State>._, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_write_state_if_value_not_changed()
    {
        await sut.SetAsync(AppId.Id, userId, "root.nested", 42, CancellationToken);
        await sut.SetAsync(AppId.Id, userId, "root.nested", 42, CancellationToken);

        var actual = await sut.GetAsync(AppId.Id, userId, CancellationToken);

        var expected =
            JsonValue.Object().Add("root",
                JsonValue.Object().Add("nested", 42));

        Assert.Equal(expected.ToString(), actual.ToString());

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<AppUISettings.State>._, CancellationToken))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_remove_nested_value()
    {
        await sut.SetAsync(AppId.Id, userId, "root.nested", 42, CancellationToken);

        await sut.RemoveAsync(AppId.Id, userId, "root.nested", CancellationToken);
        await sut.RemoveAsync(AppId.Id, userId, "key", CancellationToken);

        var actual = await sut.GetAsync(AppId.Id, userId, CancellationToken);

        var expected =
            JsonValue.Object().Add("root",
                JsonValue.Object());

        Assert.Equal(expected.ToString(), actual.ToString());

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<AppUISettings.State>._, CancellationToken))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_throw_exception_if_nested_not_an_object()
    {
        await sut.SetAsync(AppId.Id, userId, "root.nested", 42, CancellationToken);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.SetAsync(AppId.Id, userId, "root.nested.value", 42, CancellationToken));
    }

    [Fact]
    public async Task Should_do_nothing_if_deleting_and_nested_not_found()
    {
        await sut.RemoveAsync(AppId.Id, userId, "root.nested", CancellationToken);

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<AppUISettings.State>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_do_nothing_if_deleting_and_key_not_found()
    {
        await sut.RemoveAsync(AppId.Id, userId, "root", CancellationToken);

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<AppUISettings.State>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }
}
