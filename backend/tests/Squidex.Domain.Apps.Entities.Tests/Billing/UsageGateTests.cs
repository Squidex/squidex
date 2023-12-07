// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Rules;
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
    private readonly DateOnly today = new DateOnly(2020, 10, 3);
    private readonly Plan planFree = new Plan { Id = "free" };
    private readonly Plan planPaid = new Plan { Id = "paid" };
    private readonly UsageGate sut;

    public UsageGateTests()
    {
        App = App with
        {
            TeamId = default
        };

        A.CallTo(() => billingPlans.GetActualPlan(A<string>._))
            .ReturnsLazily(x => (planFree, planFree.Id));

        A.CallTo(() => billingPlans.GetActualPlan(planPaid.Id))
            .ReturnsLazily(x => (planPaid, planPaid.Id));

        A.CallTo(() => usageTracker.FallbackCategory)
            .Returns("*");

        sut = new UsageGate(AppProvider, apiUsageTracker, billingPlans, messaging, usageTracker);
    }

    [Fact]
    public async Task Should_delete_assets_usage_by_app()
    {
        await ((IAssetUsageTracker)sut).DeleteUsageAsync(AppId.Id, CancellationToken);

        A.CallTo(() => usageTracker.DeleteAsync($"{AppId.Id}_Assets", CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_delete_assets_usage()
    {
        await ((IAssetUsageTracker)sut).DeleteUsageAsync(CancellationToken);

        A.CallTo(() => usageTracker.DeleteByKeyPatternAsync("^([a-zA-Z0-9]+)_[A-Za-z]+Assets", CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_delete_rules_usage_by_app()
    {
        await ((IRuleUsageTracker)sut).DeleteUsageAsync(AppId.Id, CancellationToken);

        A.CallTo(() => usageTracker.DeleteAsync($"{AppId.Id}_Rules", CancellationToken))
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
        App = App with
        {
            TeamId = TeamId
        };

        var plan = await sut.GetPlanForAppAsync(App, false, CancellationToken);

        Assert.Equal((planFree, planFree.Id, TeamId), plan);
    }

    [Fact]
    public async Task Should_get_paid_plan_for_app()
    {
        App = App with
        {
            Plan = new AssignedPlan(User, planPaid.Id)
        };

        var plan = await sut.GetPlanForAppAsync(App, false, CancellationToken);

        Assert.Equal((planPaid, planPaid.Id, null), plan);
    }

    [Fact]
    public async Task Should_get_paid_plan_for_app_id()
    {
        App = App with
        {
            Plan = new AssignedPlan(User, planPaid.Id)
        };

        var plan = await sut.GetPlanForAppAsync(AppId.Id, false, CancellationToken);

        Assert.Equal((planPaid, planPaid.Id, null), plan);
    }

    [Fact]
    public async Task Should_get_paid_plan_for_app_with_team()
    {
        App = App with
        {
            Plan = new AssignedPlan(User, planPaid.Id), TeamId = TeamId
        };

        var plan = await sut.GetPlanForAppAsync(App, false, CancellationToken);

        Assert.Equal((planPaid, planPaid.Id, TeamId), plan);
    }

    [Fact]
    public async Task Should_block_with_true_if_over_client_limit()
    {
        App = App with
        {
            Clients = AppClients.Empty.Add(clientId, clientId).Update(clientId, apiCallsLimit: 1000)
        };

        var plan = new Plan { Id = "custom", BlockingApiCalls = 1600, MaxApiCalls = 1600 };

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
        var now = new DateOnly(2020, 10, 2);

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
    public async Task Should_track_api_request_without_team()
    {
        await sut.TrackRequestAsync(App, "client", today, 42, 50, 512, CancellationToken);

        A.CallTo(() => apiUsageTracker.TrackAsync(today, AppId.Id.ToString(), "client", 42, 50, 512, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_track_api_request_with_team()
    {
        App = App with
        {
            TeamId = TeamId
        };

        await sut.TrackRequestAsync(App, "client", today, 42, 50, 512, CancellationToken);

        A.CallTo(() => apiUsageTracker.TrackAsync(today, App.TeamId!.ToString()!, AppId.Name, 42, 50, 512, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_track_rules_usage_without_team()
    {
        Counters? countersSummary = null;
        Counters? countersDate = null;

        var ruleId = DomainId.NewGuid();

        A.CallTo(() => usageTracker.TrackAsync(default, $"{AppId.Id}_Rules", ruleId.ToString(), A<Counters>._, CancellationToken))
            .Invokes(x => countersSummary = x.GetArgument<Counters>(3));

        A.CallTo(() => usageTracker.TrackAsync(today, $"{AppId.Id}_Rules", ruleId.ToString(), A<Counters>._, CancellationToken))
            .Invokes(x => countersDate = x.GetArgument<Counters>(3));

        await ((IRuleUsageTracker)sut).TrackAsync(AppId.Id, ruleId, today, 100, 120, 140, CancellationToken);

        var expected = new Counters
        {
            [UsageGate.RulesKeys.TotalCreated] = 100,
            [UsageGate.RulesKeys.TotalSucceeded] = 120,
            [UsageGate.RulesKeys.TotalFailed] = 140
        };

        countersSummary.Should().BeEquivalentTo(expected);
        countersDate.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_track_rules_usage_with_team()
    {
        App = App with
        {
            TeamId = TeamId
        };

        Counters? countersSummary = null;
        Counters? countersDate = null;

        var ruleId = DomainId.NewGuid();

        A.CallTo(() => usageTracker.TrackAsync(default, $"{TeamId}_TeamRules", AppId.Id.ToString(), A<Counters>._, CancellationToken))
            .Invokes(x => countersSummary = x.GetArgument<Counters>(3));

        A.CallTo(() => usageTracker.TrackAsync(today, $"{TeamId}_TeamRules", AppId.Id.ToString(), A<Counters>._, CancellationToken))
            .Invokes(x => countersDate = x.GetArgument<Counters>(3));

        await ((IRuleUsageTracker)sut).TrackAsync(AppId.Id, ruleId, today, 100, 120, 140, CancellationToken);

        var expected = new Counters
        {
            [UsageGate.RulesKeys.TotalCreated] = 100,
            [UsageGate.RulesKeys.TotalSucceeded] = 120,
            [UsageGate.RulesKeys.TotalFailed] = 140
        };

        countersSummary.Should().BeEquivalentTo(expected);
        countersDate.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_get_rules_total_from_summary_date_by_app()
    {
        A.CallTo(() => usageTracker.QueryAsync($"{AppId.Id}_Rules", default, default, CancellationToken))
            .Returns(
                new Dictionary<string, List<(DateOnly, Counters)>>
                {
                    [AppId.Id.ToString()] =
                    [
                        (default, new Counters
                        {
                            [UsageGate.RulesKeys.TotalCreated] = 100,
                            [UsageGate.RulesKeys.TotalSucceeded] = 120,
                            [UsageGate.RulesKeys.TotalFailed] = 140
                        })
                    ]
                });

        var total = await ((IRuleUsageTracker)sut).GetTotalByAppAsync(AppId.Id, CancellationToken);

        total.Should().BeEquivalentTo(new Dictionary<DomainId, RuleCounters>
        {
            [AppId.Id] = new RuleCounters(100, 120, 140)
        });
    }

    [Fact]
    public async Task Should_query_rules_counters_by_app()
    {
        SetupRulesQuery($"{AppId.Id}_Rules");

        var actual = await ((IRuleUsageTracker)sut).QueryByAppAsync(AppId.Id, today, today.AddDays(2), CancellationToken);

        actual.Should().BeEquivalentTo(new List<RuleStats>
        {
            new RuleStats(today.AddDays(0), new RuleCounters(100, 120, 140)),
            new RuleStats(today.AddDays(1), new RuleCounters(200, 220, 240)),
            new RuleStats(today.AddDays(2), new RuleCounters(300, 320, 340))
        });
    }

    [Fact]
    public async Task Should_query_rules_countery_by_team()
    {
        SetupRulesQuery($"{TeamId}_TeamRules");

        var actual = await ((IRuleUsageTracker)sut).QueryByTeamAsync(TeamId, today, today.AddDays(2), CancellationToken);

        actual.Should().BeEquivalentTo(new List<RuleStats>
        {
            new RuleStats(today.AddDays(0), new RuleCounters(100, 120, 140)),
            new RuleStats(today.AddDays(1), new RuleCounters(200, 220, 240)),
            new RuleStats(today.AddDays(2), new RuleCounters(300, 320, 340))
        });
    }

    private void SetupRulesQuery(string key)
    {
        A.CallTo(() => usageTracker.QueryAsync(key, today, today.AddDays(2), CancellationToken))
            .Returns(new Dictionary<string, List<(DateOnly, Counters)>>
            {
                [usageTracker.FallbackCategory] =
                [
                    (today.AddDays(0), new Counters
                    {
                        [UsageGate.RulesKeys.TotalCreated] = 50,
                        [UsageGate.RulesKeys.TotalSucceeded] = 60,
                        [UsageGate.RulesKeys.TotalFailed] = 70
                    }),
                    (today.AddDays(1), new Counters
                    {
                        [UsageGate.RulesKeys.TotalCreated] = 200,
                        [UsageGate.RulesKeys.TotalSucceeded] = 220,
                        [UsageGate.RulesKeys.TotalFailed] = 240
                    }),
                    (today.AddDays(2), new Counters
                    {
                        [UsageGate.RulesKeys.TotalCreated] = 300,
                        [UsageGate.RulesKeys.TotalSucceeded] = 320,
                        [UsageGate.RulesKeys.TotalFailed] = 340
                    })
                ],
                ["Custom"] =
                [
                    (today.AddDays(0), new Counters
                    {
                        [UsageGate.RulesKeys.TotalCreated] = 50,
                        [UsageGate.RulesKeys.TotalSucceeded] = 60,
                        [UsageGate.RulesKeys.TotalFailed] = 70
                    })
                ]
            });
    }

    [Fact]
    public async Task Should_track_assets_usage_without_team()
    {
        Counters? countersSummary = null;
        Counters? countersDate = null;

        A.CallTo(() => usageTracker.TrackAsync(default, $"{AppId.Id}_Assets", null, A<Counters>._, CancellationToken))
            .Invokes(x => countersSummary = x.GetArgument<Counters>(3));

        A.CallTo(() => usageTracker.TrackAsync(today, $"{AppId.Id}_Assets", null, A<Counters>._, CancellationToken))
            .Invokes(x => countersDate = x.GetArgument<Counters>(3));

        await ((IAssetUsageTracker)sut).TrackAsync(AppId.Id, today, 512, 3, CancellationToken);

        var expected = new Counters
        {
            [UsageGate.AssetsKeys.TotalSize] = 512,
            [UsageGate.AssetsKeys.TotalAssets] = 3
        };

        countersSummary.Should().BeEquivalentTo(expected);
        countersDate.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_track_assets_usage_with_team()
    {
        App = App with
        {
            TeamId = TeamId
        };

        Counters? countersSummary = null;
        Counters? countersDate = null;

        A.CallTo(() => usageTracker.TrackAsync(default, $"{TeamId}_TeamAssets", AppId.Id.ToString(), A<Counters>._, CancellationToken))
            .Invokes(x => countersSummary = x.GetArgument<Counters>(3));

        A.CallTo(() => usageTracker.TrackAsync(today, $"{TeamId}_TeamAssets", AppId.Id.ToString(), A<Counters>._, CancellationToken))
            .Invokes(x => countersDate = x.GetArgument<Counters>(3));

        await ((IAssetUsageTracker)sut).TrackAsync(AppId.Id, today, 512, 3, CancellationToken);

        var expected = new Counters
        {
            [UsageGate.AssetsKeys.TotalSize] = 512,
            [UsageGate.AssetsKeys.TotalAssets] = 3
        };

        countersSummary.Should().BeEquivalentTo(expected);
        countersDate.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Should_get_assets_total_from_summary_date_by_app()
    {
        A.CallTo(() => usageTracker.GetAsync($"{AppId.Id}_Assets", default, default, null, CancellationToken))
            .Returns(new Counters
            {
                [UsageGate.AssetsKeys.TotalSize] = 2048,
                [UsageGate.AssetsKeys.TotalAssets] = 124
            });

        var total = await ((IAssetUsageTracker)sut).GetTotalByAppAsync(AppId.Id, CancellationToken);

        Assert.Equal(new AssetCounters(2048, 124), total);
    }

    [Fact]
    public async Task Should_get_assets_total_from_summary_date_by_team()
    {
        A.CallTo(() => usageTracker.GetAsync($"{AppId.Id}_TeamAssets", default, default, null, CancellationToken))
            .Returns(new Counters
            {
                [UsageGate.AssetsKeys.TotalSize] = 2048,
                [UsageGate.AssetsKeys.TotalAssets] = 124
            });

        var total = await ((IAssetUsageTracker)sut).GetTotalByTeamAsync(AppId.Id, CancellationToken);

        Assert.Equal(new AssetCounters(2048, 124), total);
    }

    [Fact]
    public async Task Should_query_assets_counters_by_app()
    {
        SetupAssetQuery($"{AppId.Id}_Assets");

        var actual = await ((IAssetUsageTracker)sut).QueryByAppAsync(AppId.Id, today, today.AddDays(2), CancellationToken);

        actual.Should().BeEquivalentTo(new List<AssetStats>
        {
            new AssetStats(today.AddDays(0), new AssetCounters(128, 2)),
            new AssetStats(today.AddDays(1), new AssetCounters(256, 3)),
            new AssetStats(today.AddDays(2), new AssetCounters(512, 4))
        });
    }

    [Fact]
    public async Task Should_query_assets_countery_by_team()
    {
        SetupAssetQuery($"{TeamId}_TeamAssets");

        var actual = await ((IAssetUsageTracker)sut).QueryByTeamAsync(TeamId, today, today.AddDays(2), CancellationToken);

        actual.Should().BeEquivalentTo(new List<AssetStats>
        {
            new AssetStats(today.AddDays(0), new AssetCounters(128, 2)),
            new AssetStats(today.AddDays(1), new AssetCounters(256, 3)),
            new AssetStats(today.AddDays(2), new AssetCounters(512, 4))
        });
    }

    private void SetupAssetQuery(string key)
    {
        A.CallTo(() => usageTracker.QueryAsync(key, today, today.AddDays(2), CancellationToken))
            .Returns(new Dictionary<string, List<(DateOnly, Counters)>>
            {
                [usageTracker.FallbackCategory] =
                [
                    (today.AddDays(0), new Counters
                    {
                        [UsageGate.AssetsKeys.TotalSize] = 64,
                        [UsageGate.AssetsKeys.TotalAssets] = 1
                    }),
                    (today.AddDays(1), new Counters
                    {
                        [UsageGate.AssetsKeys.TotalSize] = 256,
                        [UsageGate.AssetsKeys.TotalAssets] = 3
                    }),
                    (today.AddDays(2), new Counters
                    {
                        [UsageGate.AssetsKeys.TotalSize] = 512,
                        [UsageGate.AssetsKeys.TotalAssets] = 4
                    })
                ],
                ["Custom"] =
                [
                    (today.AddDays(0), new Counters
                    {
                        [UsageGate.AssetsKeys.TotalSize] = 64,
                        [UsageGate.AssetsKeys.TotalAssets] = 1
                    })
                ]
            });
    }
}
