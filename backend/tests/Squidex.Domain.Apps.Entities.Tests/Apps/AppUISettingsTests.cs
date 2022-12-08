// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Domain.Apps.Entities.Apps;

public sealed class AppUISettingsTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly TestState<AppUISettings.State> state;
    private readonly DomainId appId = DomainId.NewGuid();
    private readonly string userId = Guid.NewGuid().ToString();
    private readonly string stateId;
    private readonly AppUISettings sut;

    public AppUISettingsTests()
    {
        ct = cts.Token;

        stateId = $"{appId}_{userId}";
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
        await ((IDeleter)sut).DeleteContributorAsync(appId, userId, ct);

        A.CallTo(() => state.Persistence.DeleteAsync(ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_delete_app_and_contributors()
    {
        var app = Mocks.App(NamedId.Of(appId, "my-app"));

        A.CallTo(() => app.Contributors)
            .Returns(Contributors.Empty.Assign(userId, Role.Owner));

        var rootState = new TestState<AppUISettings.State>(appId, state.PersistenceFactory);

        await ((IDeleter)sut).DeleteAppAsync(app, ct);

        A.CallTo(() => state.Persistence.DeleteAsync(ct))
            .MustHaveHappened();

        A.CallTo(() => rootState.Persistence.DeleteAsync(ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_set_setting()
    {
        await sut.SetAsync(appId, userId, new JsonObject().Add("key", 42), ct);

        var actual = await sut.GetAsync(appId, userId, ct);

        var expected =
            new JsonObject().Add("key", 42);

        Assert.Equal(expected.ToString(), actual.ToString());

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<AppUISettings.State>._, ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_set_root_value()
    {
        await sut.SetAsync(appId, userId, "key", 42, ct);

        var actual = await sut.GetAsync(appId, userId, ct);

        var expected =
            new JsonObject().Add("key", 42);

        Assert.Equal(expected.ToString(), actual.ToString());

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<AppUISettings.State>._, ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_remove_root_value()
    {
        await sut.SetAsync(appId, userId, "key", 42, ct);

        await sut.RemoveAsync(appId, userId, "key", ct);
        await sut.RemoveAsync(appId, userId, "key", ct);

        var actual = await sut.GetAsync(appId, userId, ct);

        var expected = new JsonObject();

        Assert.Equal(expected.ToString(), actual.ToString());

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<AppUISettings.State>._, ct))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_set_nested_value()
    {
        await sut.SetAsync(appId, userId, "root.nested", 42, ct);

        var actual = await sut.GetAsync(appId, userId, ct);

        var expected =
            new JsonObject().Add("root",
                new JsonObject().Add("nested", 42));

        Assert.Equal(expected.ToString(), actual.ToString());

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<AppUISettings.State>._, ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_write_state_if_value_not_changed()
    {
        await sut.SetAsync(appId, userId, "root.nested", 42, ct);
        await sut.SetAsync(appId, userId, "root.nested", 42, ct);

        var actual = await sut.GetAsync(appId, userId, ct);

        var expected =
            new JsonObject().Add("root",
                new JsonObject().Add("nested", 42));

        Assert.Equal(expected.ToString(), actual.ToString());

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<AppUISettings.State>._, ct))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_remove_nested_value()
    {
        await sut.SetAsync(appId, userId, "root.nested", 42, ct);

        await sut.RemoveAsync(appId, userId, "root.nested", ct);
        await sut.RemoveAsync(appId, userId, "key", ct);

        var actual = await sut.GetAsync(appId, userId, ct);

        var expected =
            new JsonObject().Add("root",
                new JsonObject());

        Assert.Equal(expected.ToString(), actual.ToString());

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<AppUISettings.State>._, ct))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_throw_exception_if_nested_not_an_object()
    {
        await sut.SetAsync(appId, userId, "root.nested", 42, ct);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.SetAsync(appId, userId, "root.nested.value", 42, ct));
    }

    [Fact]
    public async Task Should_do_nothing_if_deleting_and_nested_not_found()
    {
        await sut.RemoveAsync(appId, userId, "root.nested", ct);

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<AppUISettings.State>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_do_nothing_if_deleting_and_key_not_found()
    {
        await sut.RemoveAsync(appId, userId, "root", ct);

        A.CallTo(() => state.Persistence.WriteSnapshotAsync(A<AppUISettings.State>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }
}
