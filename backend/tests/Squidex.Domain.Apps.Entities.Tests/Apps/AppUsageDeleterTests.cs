// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.UsageTracking;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public class AppUsageDeleterTests
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly CancellationToken ct;
        private readonly IApiUsageTracker usageTracker = A.Fake<IApiUsageTracker>();
        private readonly AppUsageDeleter sut;

        public AppUsageDeleterTests()
        {
            ct = cts.Token;

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
            var app = Mocks.App(NamedId.Of(DomainId.NewGuid(), "my-app"));

            await sut.DeleteAppAsync(app, ct);

            A.CallTo(() => usageTracker.DeleteAsync(app.Id.ToString(), ct))
                .MustHaveHappened();
        }
    }
}
