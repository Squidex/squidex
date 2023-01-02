// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.UsageTracking;
using Squidex.Messaging;

namespace Squidex.Domain.Apps.Entities.Billing;

public class UsageGateTests : GivenContext
{
    private readonly IMessageBus messaging = A.Fake<IMessageBus>();
    private readonly IApiUsageTracker apiUsageTracker = A.Fake<IApiUsageTracker>();
    private readonly IBillingPlans billingPlans = A.Fake<IBillingPlans>();
    private readonly IUsageTracker usageTracker = A.Fake<IUsageTracker>();
    private readonly string clientId = Guid.NewGuid().ToString();
    private readonly DomainId teamId = DomainId.NewGuid();
    private readonly DateTime today = new DateTime(2020, 10, 3);
    private readonly Plan planFree = new Plan { Id = "free" };
    private readonly Plan planPaid = new Plan { Id = "paid" };
    private readonly UsageGate sut;

    public UsageGateTests()
    {
        A.CallTo(() => billingPlans.GetActualPlan(A<string>._))
            .ReturnsLazily(x => (planFree, planFree.Id));

        A.CallTo(() => billingPlans.GetActualPlan(planPaid.Id))
            .ReturnsLazily(x => (planPaid, planPaid.Id));

        A.CallTo(() => usageTracker.FallbackCategory)
            .Returns("*");

        sut = new UsageGate(AppProvider, apiUsageTracker, billingPlans, messaging, usageTracker);
    }

    [Fact]
    public async Task Should_delete_app_asset_usage()
    {
        await sut.DeleteAssetUsageAsync(AppId.Id, CancellationToken);

        A.CallTo(() => usageTracker.DeleteAsync($"{AppId.Id}_Assets", CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_delete_assets_usage()
    {
        await sut.DeleteAssetsUsageAsync(CancellationToken);

        A.CallTo(() => usageTracker.DeleteByKeyPatternAsync("^([a-zA-Z0-9]+)_Assets", CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_get_free_plan_for_app()
    {
        var plan = await sut.GetPlanForAppAsync(App, false, CancellationToken);

        Assert.Equal((planFree, planFree.Id, null), plan);
    }

    [Fact]
    public async Task Should_get_free_plan_for_app_with_team()
    {
        var team = A.Fake<ITeamEntity>();

        A.CallTo(() => AppProvider.GetTeamAsync(teamId, CancellationToken))
            .Returns(team);

        A.CallTo(() => team.Id)
            .Returns(teamId);

        A.CallTo(() => App.TeamId)
            .Returns(teamId);

        var plan = await sut.GetPlanForAppAsync(App, false, CancellationToken);

        Assert.Equal((planFree, planFree.Id, teamId), plan);
    }

    [Fact]
    public async Task Should_get_paid_plan_for_app()
    {
        A.CallTo(() => App.Plan)
            .Returns(new AssignedPlan(RefToken.User("1"), planPaid.Id));

        var plan = await sut.GetPlanForAppAsync(App, false, CancellationToken);

        Assert.Equal((planPaid, planPaid.Id, null), plan);
    }

    [Fact]
    public async Task Should_get_paid_plan_for_app_id()
    {
        A.CallTo(() => App.Plan)
            .Returns(new AssignedPlan(RefToken.User("1"), planPaid.Id));

        var plan = await sut.GetPlanForAppAsync(AppId.Id, false, CancellationToken);

        Assert.Equal((planPaid, planPaid.Id, null), plan);
    }

    [Fact]
    public async Task Should_get_paid_plan_for_app_with_team()
    {
        var team = A.Fake<ITeamEntity>();

        A.CallTo(() => AppProvider.GetTeamAsync(teamId, CancellationToken))
            .Returns(team);

        A.CallTo(() => team.Id)
            .Returns(teamId);

        A.CallTo(() => team.Plan)
            .Returns(new AssignedPlan(RefToken.User("1"), planPaid.Id));

        A.CallTo(() => App.TeamId)
            .Returns(teamId);

        var plan = await sut.GetPlanForAppAsync(App, false, CancellationToken);

        Assert.Equal((planPaid, planPaid.Id, teamId), plan);
    }

    [Fact]
    public async Task Should_block_with_true_if_over_client_limit()
    {
        var plan = new Plan { Id = "custom", BlockingApiCalls = 1600, MaxApiCalls = 1600 };

        A.CallTo(() => billingPlans.GetActualPlan(A<string>._))
            .ReturnsLazily(x => (plan, plan.Id));

        A.CallTo(() => App.Clients)
            .Returns(AppClients.Empty.Add(clientId, clientId).Update(clientId, apiCallsLimit: 1000));

        A.CallTo(() => apiUsageTracker.GetMonthCallsAsync(AppId.Id.ToString(), today, A<string>._, CancellationToken))
            .Returns(1000);

        var isBlocked = await sut.IsBlockedAsync(App, clientId, today, CancellationToken);

        Assert.True(isBlocked);

        A.CallTo(() => messaging.PublishAsync(A<UsageTrackingCheck>._, null, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_return_true_if_over_blocking_limit()
    {
        var plan = new Plan { Id = "custom", BlockingApiCalls = 600, MaxApiCalls = 600 };

        A.CallTo(() => billingPlans.GetActualPlan(A<string>._))
            .ReturnsLazily(x => (plan, plan.Id));

        A.CallTo(() => apiUsageTracker.GetMonthCallsAsync(AppId.Id.ToString(), today, A<string>._, CancellationToken))
            .Returns(1000);

        var isBlocked = await sut.IsBlockedAsync(App, clientId, today, CancellationToken);

        Assert.True(isBlocked);

        A.CallTo(() => messaging.PublishAsync(A<UsageTrackingCheck>._, null, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_return_false_if_below_blocking_limit()
    {
        var plan = new Plan { Id = "custom", BlockingApiCalls = 1600, MaxApiCalls = 1600 };

        A.CallTo(() => billingPlans.GetActualPlan(A<string>._))
            .ReturnsLazily(x => (plan, plan.Id));

        A.CallTo(() => apiUsageTracker.GetMonthCallsAsync(AppId.Id.ToString(), today, A<string>._, CancellationToken))
            .Returns(100);

        var isBlocked = await sut.IsBlockedAsync(App, clientId, today, CancellationToken);

        Assert.False(isBlocked);

        A.CallTo(() => messaging.PublishAsync(A<UsageTrackingCheck>._, null, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_return_false_and_notify_if_about_to_over_included_contingent()
    {
        var plan = new Plan { Id = "custom", BlockingApiCalls = 5000, MaxApiCalls = 3000 };

        A.CallTo(() => billingPlans.GetActualPlan(A<string>._))
            .ReturnsLazily(x => (plan, plan.Id));

        A.CallTo(() => apiUsageTracker.GetMonthCallsAsync(AppId.Id.ToString(), today, A<string>._, CancellationToken))
            .Returns(1200); // in 10 days = 4000 / month

        var isBlocked = await sut.IsBlockedAsync(App, clientId, today, CancellationToken);

        Assert.False(isBlocked);

        A.CallTo(() => messaging.PublishAsync(A<UsageTrackingCheck>._, null, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_return_false_and_notify_if_about_to_over_included_contingent_but_no_max_given()
    {
        var plan = new Plan { Id = "custom", BlockingApiCalls = 5000, MaxApiCalls = 0 };

        A.CallTo(() => billingPlans.GetActualPlan(A<string>._))
            .ReturnsLazily(x => (plan, plan.Id));

        A.CallTo(() => apiUsageTracker.GetMonthCallsAsync(AppId.Id.ToString(), today, A<string>._, CancellationToken))
            .Returns(1200); // in 10 days = 4000 / month

        var isBlocked = await sut.IsBlockedAsync(App, clientId, today, CancellationToken);

        Assert.False(isBlocked);

        A.CallTo(() => messaging.PublishAsync(A<UsageTrackingCheck>._, null, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_only_notify_once_if_about_to_be_over_included_contingent()
    {
        var plan = new Plan { Id = "custom", BlockingApiCalls = 5000, MaxApiCalls = 3000 };

        A.CallTo(() => billingPlans.GetActualPlan(A<string>._))
            .ReturnsLazily(x => (plan, plan.Id));

        A.CallTo(() => apiUsageTracker.GetMonthCallsAsync(AppId.Id.ToString(), today, A<string>._, CancellationToken))
            .Returns(1200); // in 10 days = 4000 / month

        await sut.IsBlockedAsync(App, clientId, today, CancellationToken);
        await sut.IsBlockedAsync(App, clientId, today, CancellationToken);

        A.CallTo(() => messaging.PublishAsync(A<UsageTrackingCheck>._, null, CancellationToken))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_not_notify_if_lower_than_10_percent()
    {
        var now = new DateTime(2020, 10, 2);

        var plan = new Plan { Id = "custom", BlockingApiCalls = 5000, MaxApiCalls = 3000 };

        A.CallTo(() => billingPlans.GetActualPlan(A<string>._))
            .ReturnsLazily(x => (plan, plan.Id));

        A.CallTo(() => apiUsageTracker.GetMonthCallsAsync(AppId.Id.ToString(), now, A<string>._, CancellationToken))
            .Returns(220); // in 3 days = 3300 / month

        await sut.IsBlockedAsync(App, clientId, now, CancellationToken);
        await sut.IsBlockedAsync(App, clientId, now, CancellationToken);

        A.CallTo(() => messaging.PublishAsync(A<UsageTrackingCheck>._, null, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_get_app_asset_total_size_from_summary_date()
    {
        A.CallTo(() => usageTracker.GetAsync($"{AppId.Id}_Assets", default, default, null, CancellationToken))
            .Returns(new Counters { ["TotalSize"] = 2048 });

        var size = await sut.GetTotalSizeByAppAsync(AppId.Id, CancellationToken);

        Assert.Equal(2048, size);
    }

    [Fact]
    public async Task Should_get_team_asset_total_size_from_summary_date()
    {
        A.CallTo(() => usageTracker.GetAsync($"{AppId.Id}_TeamAssets", default, default, null, CancellationToken))
            .Returns(new Counters { ["TotalSize"] = 2048 });

        var size = await sut.GetTotalSizeByTeamAsync(AppId.Id, CancellationToken);

        Assert.Equal(2048, size);
    }

    [Fact]
    public async Task Should_track_request_async()
    {
        await sut.TrackRequestAsync(App, "client", today, 42, 50, 512, CancellationToken);

        A.CallTo(() => apiUsageTracker.TrackAsync(today, AppId.Id.ToString(), "client", 42, 50, 512, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_track_request_for_team_async()
    {
        A.CallTo(() => App.TeamId)
            .Returns(teamId);

        await sut.TrackRequestAsync(App, "client", today, 42, 50, 512, CancellationToken);

        A.CallTo(() => apiUsageTracker.TrackAsync(today, App.TeamId!.ToString()!, AppId.Name, 42, 50, 512, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_get_app_asset_counters_from_categories()
    {
        SetupAssetQuery($"{AppId.Id}_Assets");

        var actual = await sut.QueryByAppAsync(AppId.Id, today, today.AddDays(3), CancellationToken);

        actual.Should().BeEquivalentTo(new List<AssetStats>
        {
            new AssetStats(today.AddDays(0), 2, 128),
            new AssetStats(today.AddDays(1), 3, 256),
            new AssetStats(today.AddDays(2), 4, 512)
        });
    }

    [Fact]
    public async Task Should_get_team_asset_counters_from_categories()
    {
        SetupAssetQuery($"{AppId.Id}_TeamAssets");

        var actual = await sut.QueryByTeamAsync(AppId.Id, today, today.AddDays(3), CancellationToken);

        actual.Should().BeEquivalentTo(new List<AssetStats>
        {
            new AssetStats(today.AddDays(0), 2, 128),
            new AssetStats(today.AddDays(1), 3, 256),
            new AssetStats(today.AddDays(2), 4, 512)
        });
    }

    private void SetupAssetQuery(string key)
    {
        A.CallTo(() => usageTracker.QueryAsync(key, today, today.AddDays(3), CancellationToken))
            .Returns(new Dictionary<string, List<(DateTime, Counters)>>
            {
                [usageTracker.FallbackCategory] = new List<(DateTime, Counters)>
                {
                    (today.AddDays(0), new Counters
                    {
                        ["TotalSize"] = 128,
                        ["TotalAssets"] = 2
                    }),
                    (today.AddDays(1), new Counters
                    {
                        ["TotalSize"] = 256,
                        ["TotalAssets"] = 3
                    }),
                    (today.AddDays(2), new Counters
                    {
                        ["TotalSize"] = 512,
                        ["TotalAssets"] = 4
                    })
                }
            });
    }

    [Fact]
    public async Task Should_increase_usage_for_asset_event()
    {
        Counters? countersSummary = null;
        Counters? countersDate = null;

        A.CallTo(() => usageTracker.TrackAsync(default, $"{AppId.Id}_Assets", null, A<Counters>._, CancellationToken))
            .Invokes(x => countersSummary = x.GetArgument<Counters>(3));

        A.CallTo(() => usageTracker.TrackAsync(today, $"{AppId.Id}_Assets", null, A<Counters>._, CancellationToken))
            .Invokes(x => countersDate = x.GetArgument<Counters>(3));

        await sut.TrackAssetAsync(AppId.Id, today, 512, 3, CancellationToken);

        var expected = new Counters
        {
            ["TotalSize"] = 512,
            ["TotalAssets"] = 3
        };

        countersSummary.Should().BeEquivalentTo(expected);
        countersDate.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_increase_team_usage_for_asset_event_and_team_app()
    {
        Counters? countersSummary = null;
        Counters? countersDate = null;

        var team = A.Fake<ITeamEntity>();

        A.CallTo(() => team.Id)
            .Returns(teamId);

        A.CallTo(() => App.TeamId)
            .Returns(teamId);

        A.CallTo(() => AppProvider.GetTeamAsync(teamId, CancellationToken))
            .Returns(team);

        A.CallTo(() => usageTracker.TrackAsync(default, $"{teamId}_TeamAssets", null, A<Counters>._, CancellationToken))
            .Invokes(x => countersSummary = x.GetArgument<Counters>(3));

        A.CallTo(() => usageTracker.TrackAsync(today, $"{teamId}_TeamAssets", null, A<Counters>._, CancellationToken))
            .Invokes(x => countersDate = x.GetArgument<Counters>(3));

        await sut.TrackAssetAsync(AppId.Id, today, 512, 3, CancellationToken);

        var expected = new Counters
        {
            ["TotalSize"] = 512,
            ["TotalAssets"] = 3
        };

        countersSummary.Should().BeEquivalentTo(expected);
        countersDate.Should().BeEquivalentTo(expected);
    }
}
