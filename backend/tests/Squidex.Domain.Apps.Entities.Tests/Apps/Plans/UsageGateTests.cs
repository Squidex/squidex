// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.UsageTracking;
using Squidex.Messaging;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Plans
{
    public class UsageGateTests
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly CancellationToken ct;
        private readonly IAppEntity appEntity;
        private readonly IAppLimitsPlan appPlan = A.Fake<IAppLimitsPlan>();
        private readonly IAppPlansProvider appPlansProvider = A.Fake<IAppPlansProvider>();
        private readonly IApiUsageTracker usageTracker = A.Fake<IApiUsageTracker>();
        private readonly IMessageBus messaging = A.Fake<IMessageBus>();
        private readonly string clientId = Guid.NewGuid().ToString();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly UsageGate sut;
        private DateTime today = new DateTime(2020, 10, 3);
        private long apiCallsBlocking;
        private long apiCallsMax;
        private long apiCallsCurrent;

        public UsageGateTests()
        {
            appEntity = Mocks.App(appId);

            ct = cts.Token;

            A.CallTo(() => appPlansProvider.GetPlan(null))
                .Returns(appPlan);

            A.CallTo(() => appPlansProvider.GetPlanForApp(appEntity))
                .Returns((appPlan, "free"));

            A.CallTo(() => appPlan.MaxApiCalls)
                .ReturnsLazily(x => apiCallsMax);

            A.CallTo(() => appPlan.BlockingApiCalls)
                .ReturnsLazily(x => apiCallsBlocking);

            A.CallTo(() => usageTracker.GetMonthCallsAsync(appId.Id.ToString(), today, A<string>._, ct))
                .ReturnsLazily(x => Task.FromResult(apiCallsCurrent));

            sut = new UsageGate(appPlansProvider, usageTracker, messaging);
        }

        [Fact]
        public async Task Should_return_true_if_over_client_limit()
        {
            A.CallTo(() => appEntity.Clients)
                .Returns(AppClients.Empty.Add(clientId, clientId).Update(clientId, apiCallsLimit: 1000));

            apiCallsCurrent = 1000;
            apiCallsBlocking = 1600;
            apiCallsMax = 1600;

            var isBlocked = await sut.IsBlockedAsync(appEntity, clientId, today, ct);

            Assert.True(isBlocked);

            A.CallTo(() => messaging.PublishAsync(A<UsageTrackingCheck>._, null, ct))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_return_true_if_over_blocking_limit()
        {
            apiCallsCurrent = 1000;
            apiCallsBlocking = 600;
            apiCallsMax = 600;

            var isBlocked = await sut.IsBlockedAsync(appEntity, clientId, today, ct);

            Assert.True(isBlocked);

            A.CallTo(() => messaging.PublishAsync(A<UsageTrackingCheck>._, null, ct))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_return_false_if_below_blocking_limit()
        {
            apiCallsCurrent = 100;
            apiCallsBlocking = 1600;
            apiCallsMax = 1600;

            var isBlocked = await sut.IsBlockedAsync(appEntity, clientId, today, ct);

            Assert.False(isBlocked);

            A.CallTo(() => messaging.PublishAsync(A<UsageTrackingCheck>._, null, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_return_false_and_notify_if_about_to_over_included_contingent()
        {
            apiCallsCurrent = 1200; // in 10 days = 4000 / month
            apiCallsBlocking = 5000;
            apiCallsMax = 3000;

            var isBlocked = await sut.IsBlockedAsync(appEntity, clientId, today, ct);

            Assert.False(isBlocked);

            A.CallTo(() => messaging.PublishAsync(A<UsageTrackingCheck>._, null, ct))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_return_false_and_notify_if_about_to_over_included_contingent_but_no_max_given()
        {
            apiCallsCurrent = 1200; // in 10 days = 4000 / month
            apiCallsBlocking = 5000;
            apiCallsMax = 0;

            var isBlocked = await sut.IsBlockedAsync(appEntity, clientId, today, ct);

            Assert.False(isBlocked);

            A.CallTo(() => messaging.PublishAsync(A<UsageTrackingCheck>._, null, ct))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_only_notify_once_if_about_to_be_over_included_contingent()
        {
            apiCallsCurrent = 1200; // in 10 days = 4000 / month
            apiCallsBlocking = 5000;
            apiCallsMax = 3000;

            await sut.IsBlockedAsync(appEntity, clientId, today, ct);
            await sut.IsBlockedAsync(appEntity, clientId, today, ct);

            A.CallTo(() => messaging.PublishAsync(A<UsageTrackingCheck>._, null, ct))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Should_not_notify_if_lower_than_10_percent()
        {
            today = new DateTime(2020, 10, 2);

            apiCallsCurrent = 220; // in 3 days = 3300 / month
            apiCallsBlocking = 5000;
            apiCallsMax = 3000;

            await sut.IsBlockedAsync(appEntity, clientId, today, ct);
            await sut.IsBlockedAsync(appEntity, clientId, today, ct);

            A.CallTo(() => messaging.PublishAsync(A<UsageTrackingCheck>._, null, A<CancellationToken>._))
                .MustNotHaveHappened();
        }
    }
}
