// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.UsageTracking;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Plans
{
    public class UsageGateTests
    {
        private readonly IAppEntity appEntity;
        private readonly IAppLimitsPlan appPlan = A.Fake<IAppLimitsPlan>();
        private readonly IAppPlansProvider appPlansProvider = A.Fake<IAppPlansProvider>();
        private readonly IApiUsageTracker usageTracker = A.Fake<IApiUsageTracker>();
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly IUsageNotifierGrain usageNotifierGrain = A.Fake<IUsageNotifierGrain>();
        private readonly DateTime today = new DateTime(2020, 04, 10);
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly UsageGate sut;
        private long apiCallsBlocking;
        private long apiCallsMax;
        private long apiCallsCurrent;

        public UsageGateTests()
        {
            appEntity = Mocks.App(appId);

            A.CallTo(() => grainFactory.GetGrain<IUsageNotifierGrain>(SingleGrain.Id, null))
                .Returns(usageNotifierGrain);

            A.CallTo(() => appPlansProvider.GetPlan(null))
                .Returns(appPlan);

            A.CallTo(() => appPlansProvider.GetPlanForApp(appEntity))
                .Returns((appPlan, "free"));

            A.CallTo(() => appPlan.MaxApiCalls)
                .ReturnsLazily(x => apiCallsMax);

            A.CallTo(() => appPlan.BlockingApiCalls)
                .ReturnsLazily(x => apiCallsBlocking);

            A.CallTo(() => usageTracker.GetMonthCostsAsync(appId.Id.ToString(), today))
                .ReturnsLazily(x => Task.FromResult(apiCallsCurrent));

            sut = new UsageGate(appPlansProvider, usageTracker, grainFactory);
        }

        [Fact]
        public async Task Should_return_true_if_over_blocking_limt()
        {
            apiCallsCurrent = 1000;
            apiCallsBlocking = 600;
            apiCallsMax = 600;

            var isBlocked = await sut.IsBlockedAsync(appEntity, today);

            Assert.True(isBlocked);

            A.CallTo(() => usageNotifierGrain.NotifyAsync(A<UsageNotification>.Ignored))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_return_false_if_below_blocking_limit()
        {
            apiCallsCurrent = 100;
            apiCallsBlocking = 1600;
            apiCallsMax = 1600;

            var isBlocked = await sut.IsBlockedAsync(appEntity, today);

            Assert.False(isBlocked);

            A.CallTo(() => usageNotifierGrain.NotifyAsync(A<UsageNotification>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_return_false_and_notify_if_about_to_over_included_contingent()
        {
            apiCallsCurrent = 1200; // * 30 days = 3600
            apiCallsBlocking = 5000;
            apiCallsMax = 3000;

            var isBlocked = await sut.IsBlockedAsync(appEntity, today);

            Assert.False(isBlocked);

            A.CallTo(() => usageNotifierGrain.NotifyAsync(A<UsageNotification>.Ignored))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_return_false_and_notify_if_about_to_over_included_contingent_but_no_max_given()
        {
            apiCallsCurrent = 1200; // * 30 days = 3600
            apiCallsBlocking = 5000;
            apiCallsMax = 0;

            var isBlocked = await sut.IsBlockedAsync(appEntity, today);

            Assert.False(isBlocked);

            A.CallTo(() => usageNotifierGrain.NotifyAsync(A<UsageNotification>.Ignored))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_only_notify_once_if_about_to_be_over_included_contingent()
        {
            apiCallsCurrent = 1200; // * 30 days = 3600
            apiCallsBlocking = 5000;
            apiCallsMax = 3000;

            await sut.IsBlockedAsync(appEntity, today);
            await sut.IsBlockedAsync(appEntity, today);

            A.CallTo(() => usageNotifierGrain.NotifyAsync(A<UsageNotification>.Ignored))
                .MustHaveHappenedOnceExactly();
        }
    }
}
