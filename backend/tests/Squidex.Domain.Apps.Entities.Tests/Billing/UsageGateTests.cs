// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Teams;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.UsageTracking;
using Squidex.Messaging;

namespace Squidex.Domain.Apps.Entities.Billing;

public class UsageGateTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly IMessageBus messaging = A.Fake<IMessageBus>();
    private readonly IApiUsageTracker apiUsageTracker = A.Fake<IApiUsageTracker>();
    private readonly IAppEntity appWithoutTeam;
    private readonly IAppEntity appWithTeam;
    private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
    private readonly IBillingPlans billingPlans = A.Fake<IBillingPlans>();
    private readonly IUsageTracker usageTracker = A.Fake<IUsageTracker>();
    private readonly string clientId = Guid.NewGuid().ToString();
    private readonly DomainId teamId = DomainId.NewGuid();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly DateTime today = new DateTime(2020, 10, 3);
    private readonly Plan planFree = new Plan { Id = "free" };
    private readonly Plan planPaid = new Plan { Id = "paid" };
    private readonly UsageGate sut;

    public UsageGateTests()
    {
        appWithoutTeam = Mocks.App(appId);
        appWithTeam = Mocks.App(appId);

        ct = cts.Token;

        A.CallTo(() => appWithTeam.TeamId)
            .Returns(teamId);

        A.CallTo(() => billingPlans.GetActualPlan(A<string>._))
            .ReturnsLazily(x => (planFree, planFree.Id));

        A.CallTo(() => billingPlans.GetActualPlan(planPaid.Id))
            .ReturnsLazily(x => (planPaid, planPaid.Id));

        A.CallTo(() => usageTracker.FallbackCategory)
            .Returns("*");

        sut = new UsageGate(appProvider, apiUsageTracker, billingPlans, messaging, usageTracker);
    }

    [Fact]
    public async Task Should_delete_app_asset_usage()
    {
        await sut.DeleteAssetUsageAsync(appId.Id, ct);

        A.CallTo(() => usageTracker.DeleteAsync($"{appId.Id}_Assets", ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_delete_assets_usage()
    {
        await sut.DeleteAssetsUsageAsync(ct);

        A.CallTo(() => usageTracker.DeleteByKeyPatternAsync("^([a-zA-Z0-9]+)_Assets", ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_get_free_plan_for_app()
    {
        var plan = await sut.GetPlanForAppAsync(appWithoutTeam, false, ct);

        Assert.Equal((planFree, planFree.Id, null), plan);
    }

    [Fact]
    public async Task Should_get_free_plan_for_app_with_team()
    {
        var team = A.Fake<ITeamEntity>();

        A.CallTo(() => appProvider.GetTeamAsync(teamId, ct))
            .Returns(team);

        A.CallTo(() => team.Id)
            .Returns(teamId);

        var plan = await sut.GetPlanForAppAsync(appWithTeam, false, ct);

        Assert.Equal((planFree, planFree.Id, teamId), plan);
    }

    [Fact]
    public async Task Should_get_paid_plan_for_app()
    {
        A.CallTo(() => appWithoutTeam.Plan)
            .Returns(new AssignedPlan(RefToken.User("1"), planPaid.Id));

        var plan = await sut.GetPlanForAppAsync(appWithoutTeam, false, ct);

        Assert.Equal((planPaid, planPaid.Id, null), plan);
    }

    [Fact]
    public async Task Should_get_paid_plan_for_app_id()
    {
        A.CallTo(() => appProvider.GetAppAsync(appWithoutTeam.Id, true, ct))
            .Returns(appWithoutTeam);

        A.CallTo(() => appWithoutTeam.Plan)
            .Returns(new AssignedPlan(RefToken.User("1"), planPaid.Id));

        var plan = await sut.GetPlanForAppAsync(appWithoutTeam.Id, false, ct);

        Assert.Equal((planPaid, planPaid.Id, null), plan);
    }

    [Fact]
    public async Task Should_get_paid_plan_for_app_with_team()
    {
        var team = A.Fake<ITeamEntity>();

        A.CallTo(() => appProvider.GetTeamAsync(teamId, ct))
            .Returns(team);

        A.CallTo(() => team.Id)
            .Returns(teamId);

        A.CallTo(() => team.Plan)
            .Returns(new AssignedPlan(RefToken.User("1"), planPaid.Id));

        var plan = await sut.GetPlanForAppAsync(appWithTeam, false, ct);

        Assert.Equal((planPaid, planPaid.Id, teamId), plan);
    }

    [Fact]
    public async Task Should_block_with_true_if_over_client_limit()
    {
        var plan = new Plan { Id = "custom", BlockingApiCalls = 1600, MaxApiCalls = 1600 };

        A.CallTo(() => billingPlans.GetActualPlan(A<string>._))
            .ReturnsLazily(x => (plan, plan.Id));

        A.CallTo(() => appWithoutTeam.Clients)
            .Returns(AppClients.Empty.Add(clientId, clientId).Update(clientId, apiCallsLimit: 1000));

        A.CallTo(() => apiUsageTracker.GetMonthCallsAsync(appId.Id.ToString(), today, A<string>._, ct))
            .Returns(1000);

        var isBlocked = await sut.IsBlockedAsync(appWithoutTeam, clientId, today, ct);

        Assert.True(isBlocked);

        A.CallTo(() => messaging.PublishAsync(A<UsageTrackingCheck>._, null, ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_return_true_if_over_blocking_limit()
    {
        var plan = new Plan { Id = "custom", BlockingApiCalls = 600, MaxApiCalls = 600 };

        A.CallTo(() => billingPlans.GetActualPlan(A<string>._))
            .ReturnsLazily(x => (plan, plan.Id));

        A.CallTo(() => apiUsageTracker.GetMonthCallsAsync(appId.Id.ToString(), today, A<string>._, ct))
            .Returns(1000);

        var isBlocked = await sut.IsBlockedAsync(appWithoutTeam, clientId, today, ct);

        Assert.True(isBlocked);

        A.CallTo(() => messaging.PublishAsync(A<UsageTrackingCheck>._, null, ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_return_false_if_below_blocking_limit()
    {
        var plan = new Plan { Id = "custom", BlockingApiCalls = 1600, MaxApiCalls = 1600 };

        A.CallTo(() => billingPlans.GetActualPlan(A<string>._))
            .ReturnsLazily(x => (plan, plan.Id));

        A.CallTo(() => apiUsageTracker.GetMonthCallsAsync(appId.Id.ToString(), today, A<string>._, ct))
            .Returns(100);

        var isBlocked = await sut.IsBlockedAsync(appWithoutTeam, clientId, today, ct);

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

        A.CallTo(() => apiUsageTracker.GetMonthCallsAsync(appId.Id.ToString(), today, A<string>._, ct))
            .Returns(1200); // in 10 days = 4000 / month

        var isBlocked = await sut.IsBlockedAsync(appWithoutTeam, clientId, today, ct);

        Assert.False(isBlocked);

        A.CallTo(() => messaging.PublishAsync(A<UsageTrackingCheck>._, null, ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_return_false_and_notify_if_about_to_over_included_contingent_but_no_max_given()
    {
        var plan = new Plan { Id = "custom", BlockingApiCalls = 5000, MaxApiCalls = 0 };

        A.CallTo(() => billingPlans.GetActualPlan(A<string>._))
            .ReturnsLazily(x => (plan, plan.Id));

        A.CallTo(() => apiUsageTracker.GetMonthCallsAsync(appId.Id.ToString(), today, A<string>._, ct))
            .Returns(1200); // in 10 days = 4000 / month

        var isBlocked = await sut.IsBlockedAsync(appWithoutTeam, clientId, today, ct);

        Assert.False(isBlocked);

        A.CallTo(() => messaging.PublishAsync(A<UsageTrackingCheck>._, null, ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_only_notify_once_if_about_to_be_over_included_contingent()
    {
        var plan = new Plan { Id = "custom", BlockingApiCalls = 5000, MaxApiCalls = 3000 };

        A.CallTo(() => billingPlans.GetActualPlan(A<string>._))
            .ReturnsLazily(x => (plan, plan.Id));

        A.CallTo(() => apiUsageTracker.GetMonthCallsAsync(appId.Id.ToString(), today, A<string>._, ct))
            .Returns(1200); // in 10 days = 4000 / month

        await sut.IsBlockedAsync(appWithoutTeam, clientId, today, ct);
        await sut.IsBlockedAsync(appWithoutTeam, clientId, today, ct);

        A.CallTo(() => messaging.PublishAsync(A<UsageTrackingCheck>._, null, ct))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_not_notify_if_lower_than_10_percent()
    {
        var now = new DateTime(2020, 10, 2);

        var plan = new Plan { Id = "custom", BlockingApiCalls = 5000, MaxApiCalls = 3000 };

        A.CallTo(() => billingPlans.GetActualPlan(A<string>._))
            .ReturnsLazily(x => (plan, plan.Id));

        A.CallTo(() => apiUsageTracker.GetMonthCallsAsync(appId.Id.ToString(), now, A<string>._, ct))
            .Returns(220); // in 3 days = 3300 / month

        await sut.IsBlockedAsync(appWithoutTeam, clientId, now, ct);
        await sut.IsBlockedAsync(appWithoutTeam, clientId, now, ct);

        A.CallTo(() => messaging.PublishAsync(A<UsageTrackingCheck>._, null, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_get_app_asset_total_size_from_summary_date()
    {
        A.CallTo(() => usageTracker.GetAsync($"{appId.Id}_Assets", default, default, null, ct))
            .Returns(new Counters { ["TotalSize"] = 2048 });

        var size = await sut.GetTotalSizeByAppAsync(appId.Id, ct);

        Assert.Equal(2048, size);
    }

    [Fact]
    public async Task Should_get_team_asset_total_size_from_summary_date()
    {
        A.CallTo(() => usageTracker.GetAsync($"{appId.Id}_TeamAssets", default, default, null, ct))
            .Returns(new Counters { ["TotalSize"] = 2048 });

        var size = await sut.GetTotalSizeByTeamAsync(appId.Id, ct);

        Assert.Equal(2048, size);
    }

    [Fact]
    public async Task Should_track_request_async()
    {
        await sut.TrackRequestAsync(appWithoutTeam, "client", today, 42, 50, 512, ct);

        A.CallTo(() => apiUsageTracker.TrackAsync(today, appWithoutTeam.Id.ToString(), "client", 42, 50, 512, ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_track_request_for_team_async()
    {
        await sut.TrackRequestAsync(appWithTeam, "client", today, 42, 50, 512, ct);

        A.CallTo(() => apiUsageTracker.TrackAsync(today, appWithTeam.TeamId!.ToString()!, appWithTeam.Name, 42, 50, 512, ct))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_get_app_asset_counters_from_categories()
    {
        SetupAssetQuery($"{appId.Id}_Assets");

        var actual = await sut.QueryByAppAsync(appId.Id, today, today.AddDays(3), ct);

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
        SetupAssetQuery($"{appId.Id}_TeamAssets");

        var actual = await sut.QueryByTeamAsync(appId.Id, today, today.AddDays(3), ct);

        actual.Should().BeEquivalentTo(new List<AssetStats>
        {
            new AssetStats(today.AddDays(0), 2, 128),
            new AssetStats(today.AddDays(1), 3, 256),
            new AssetStats(today.AddDays(2), 4, 512)
        });
    }

    private void SetupAssetQuery(string key)
    {
        A.CallTo(() => usageTracker.QueryAsync(key, today, today.AddDays(3), ct))
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

        A.CallTo(() => usageTracker.TrackAsync(default, $"{appId.Id}_Assets", null, A<Counters>._, ct))
            .Invokes(x => countersSummary = x.GetArgument<Counters>(3));

        A.CallTo(() => usageTracker.TrackAsync(today, $"{appId.Id}_Assets", null, A<Counters>._, ct))
            .Invokes(x => countersDate = x.GetArgument<Counters>(3));

        await sut.TrackAssetAsync(appWithoutTeam.Id, today, 512, 3, ct);

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

        A.CallTo(() => appProvider.GetAppAsync(appWithTeam.Id, true, ct))
            .Returns(appWithTeam);

        A.CallTo(() => appProvider.GetTeamAsync(teamId, ct))
            .Returns(team);

        A.CallTo(() => usageTracker.TrackAsync(default, $"{teamId}_TeamAssets", null, A<Counters>._, ct))
            .Invokes(x => countersSummary = x.GetArgument<Counters>(3));

        A.CallTo(() => usageTracker.TrackAsync(today, $"{teamId}_TeamAssets", null, A<Counters>._, ct))
            .Invokes(x => countersDate = x.GetArgument<Counters>(3));

        await sut.TrackAssetAsync(appWithTeam.Id, today, 512, 3, ct);

        var expected = new Counters
        {
            ["TotalSize"] = 512,
            ["TotalAssets"] = 3
        };

        countersSummary.Should().BeEquivalentTo(expected);
        countersDate.Should().BeEquivalentTo(expected);
    }
}
