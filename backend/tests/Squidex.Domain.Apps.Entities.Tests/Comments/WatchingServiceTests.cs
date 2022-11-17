// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure;
using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Domain.Apps.Entities.Comments;

public class WatchingServiceTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly TestState<WatchingService.State> state1;
    private readonly TestState<WatchingService.State> state2;
    private readonly IClock clock = A.Fake<IClock>();
    private readonly DomainId appId = DomainId.NewGuid();
    private readonly string resource1 = "resource1";
    private readonly string resource2 = "resource2";
    private readonly WatchingService sut;
    private Instant now = SystemClock.Instance.GetCurrentInstant();

    public WatchingServiceTests()
    {
        ct = cts.Token;

        A.CallTo(() => clock.GetCurrentInstant())
            .ReturnsLazily(() => now);

        state1 = new TestState<WatchingService.State>($"{appId}_{resource1}");
        state2 = new TestState<WatchingService.State>($"{appId}_{resource2}", state1.PersistenceFactory);

        sut = new WatchingService(state1.PersistenceFactory)
        {
            Clock = clock
        };
    }

    [Fact]
    public async Task Should_only_return_self_if_no_one_watching()
    {
        var watching = await sut.GetWatchingUsersAsync(appId, resource1, "user1", ct);

        Assert.Equal(new[] { "user1" }, watching);
    }

    [Fact]
    public async Task Should_return_users_watching_on_same_resource()
    {
        await sut.GetWatchingUsersAsync(appId, resource1, "user1", ct);
        await sut.GetWatchingUsersAsync(appId, resource2, "user2", ct);

        var watching1 = await sut.GetWatchingUsersAsync(appId, resource1, "user3", ct);
        var watching2 = await sut.GetWatchingUsersAsync(appId, resource2, "user4", ct);

        Assert.Equal(new[] { "user1", "user3" }, watching1);
        Assert.Equal(new[] { "user2", "user4" }, watching2);
    }

    [Fact]
    public async Task Should_cleanup_old_users()
    {
        await sut.GetWatchingUsersAsync(appId, resource1, "user1", ct);
        await sut.GetWatchingUsersAsync(appId, resource2, "user2", ct);

        now = now.Plus(Duration.FromMinutes(2));

        await sut.GetWatchingUsersAsync(appId, resource1, "user3", ct);
        await sut.GetWatchingUsersAsync(appId, resource2, "user4", ct);

        var watching1 = await sut.GetWatchingUsersAsync(appId, resource1, "user5", ct);
        var watching2 = await sut.GetWatchingUsersAsync(appId, resource2, "user6", ct);

        Assert.Equal(new[] { "user3", "user5" }, watching1);
        Assert.Equal(new[] { "user4", "user6" }, watching2);

        A.CallTo(() => state1.Persistence.WriteSnapshotAsync(A<WatchingService.State>._, ct))
            .MustHaveHappened();

        A.CallTo(() => state2.Persistence.WriteSnapshotAsync(A<WatchingService.State>._, ct))
            .MustHaveHappened();
    }
}
