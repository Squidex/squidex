// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Domain.Apps.Entities.Comments;

public class WatchingServiceTests : GivenContext
{
    private readonly TestState<WatchingService.State> state1;
    private readonly TestState<WatchingService.State> state2;
    private readonly IClock clock = A.Fake<IClock>();
    private readonly string resource1 = "resource1";
    private readonly string resource2 = "resource2";
    private readonly WatchingService sut;
    private Instant now = SystemClock.Instance.GetCurrentInstant();

    public WatchingServiceTests()
    {
        A.CallTo(() => clock.GetCurrentInstant())
            .ReturnsLazily(() => now);

        state1 = new TestState<WatchingService.State>($"{AppId.Id}_{resource1}");
        state2 = new TestState<WatchingService.State>($"{AppId.Id}_{resource2}", state1.PersistenceFactory);

        sut = new WatchingService(state1.PersistenceFactory)
        {
            Clock = clock
        };
    }

    [Fact]
    public async Task Should_only_return_self_if_no_one_watching()
    {
        var watching = await sut.GetWatchingUsersAsync(AppId.Id, resource1, "user1", CancellationToken);

        Assert.Equal(new[] { "user1" }, watching);
    }

    [Fact]
    public async Task Should_return_users_watching_on_same_resource()
    {
        await sut.GetWatchingUsersAsync(AppId.Id, resource1, "user1", CancellationToken);
        await sut.GetWatchingUsersAsync(AppId.Id, resource2, "user2", CancellationToken);

        var watching1 = await sut.GetWatchingUsersAsync(AppId.Id, resource1, "user3", CancellationToken);
        var watching2 = await sut.GetWatchingUsersAsync(AppId.Id, resource2, "user4", CancellationToken);

        Assert.Equal(new[] { "user1", "user3" }, watching1);
        Assert.Equal(new[] { "user2", "user4" }, watching2);
    }

    [Fact]
    public async Task Should_cleanup_old_users()
    {
        await sut.GetWatchingUsersAsync(AppId.Id, resource1, "user1", CancellationToken);
        await sut.GetWatchingUsersAsync(AppId.Id, resource2, "user2", CancellationToken);

        now = now.Plus(Duration.FromMinutes(2));

        await sut.GetWatchingUsersAsync(AppId.Id, resource1, "user3", CancellationToken);
        await sut.GetWatchingUsersAsync(AppId.Id, resource2, "user4", CancellationToken);

        var watching1 = await sut.GetWatchingUsersAsync(AppId.Id, resource1, "user5", CancellationToken);
        var watching2 = await sut.GetWatchingUsersAsync(AppId.Id, resource2, "user6", CancellationToken);

        Assert.Equal(new[] { "user3", "user5" }, watching1);
        Assert.Equal(new[] { "user4", "user6" }, watching2);

        A.CallTo(() => state1.Persistence.WriteSnapshotAsync(A<WatchingService.State>._, CancellationToken))
            .MustHaveHappened();

        A.CallTo(() => state2.Persistence.WriteSnapshotAsync(A<WatchingService.State>._, CancellationToken))
            .MustHaveHappened();
    }
}
