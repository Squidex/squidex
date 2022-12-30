// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.UsageTracking;

namespace Squidex.Domain.Apps.Entities.Apps;

public class AppUsageDeleterTests : GivenContext
{
    private readonly IApiUsageTracker usageTracker = A.Fake<IApiUsageTracker>();
    private readonly AppUsageDeleter sut;

    public AppUsageDeleterTests()
    {
        sut = new AppUsageDeleter(usageTracker);
    }

    [Fact]
    public void Should_run_with_default_order()
    {
        var order = ((IDeleter)sut).Order;

        Assert.Equal(0, order);
    }

    [Fact]
    public async Task Should_remove_events_from_streams()
    {
        await sut.DeleteAppAsync(App, CancellationToken);

        A.CallTo(() => usageTracker.DeleteAsync(AppId.Id.ToString(), CancellationToken))
            .MustHaveHappened();
    }
}
