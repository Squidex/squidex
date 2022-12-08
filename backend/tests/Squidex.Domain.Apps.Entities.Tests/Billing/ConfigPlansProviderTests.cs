// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Billing;

public class ConfigPlansProviderTests
{
    private static readonly Plan InfinitePlan = new Plan
    {
        Id = "infinite",
        Name = "Infinite",
        MaxApiCalls = -1,
        MaxAssetSize = -1,
        MaxContributors = -1,
        BlockingApiCalls = -1
    };

    private static readonly Plan FreePlan = new Plan
    {
        Id = "free",
        Name = "Free",
        MaxApiCalls = 50000,
        MaxAssetSize = 1024 * 1024 * 10,
        MaxContributors = 2,
        BlockingApiCalls = 50000,
        IsFree = true
    };

    private static readonly Plan BasicPlan = new Plan
    {
        Id = "basic",
        Name = "Basic",
        MaxApiCalls = 150000,
        MaxAssetSize = 1024 * 1024 * 2,
        MaxContributors = 5,
        YearlyCosts = "100€",
        YearlyId = "basic_yearly",
        BlockingApiCalls = 150000,
        IsFree = false
    };

    private static readonly Plan[] Plans = { BasicPlan, FreePlan };

    [Fact]
    public void Should_return_plans()
    {
        var sut = new ConfigPlansProvider(Plans);

        sut.GetAvailablePlans().Should().BeEquivalentTo(Plans.OrderBy(x => x.MaxApiCalls));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("my-plan")]
    public void Should_return_infinite_if_nothing_configured(string planId)
    {
        var sut = new ConfigPlansProvider(Enumerable.Empty<Plan>());

        var actual = sut.GetActualPlan(planId);

        actual.Should().BeEquivalentTo((InfinitePlan, "infinite"));
    }

    [Fact]
    public void Should_return_free_plan()
    {
        var sut = new ConfigPlansProvider(Plans);

        var plan = sut.GetFreePlan();

        plan.Should().BeEquivalentTo(FreePlan);
    }

    [Fact]
    public void Should_return_infinite_plan_for_free_plan_if_not_found()
    {
        var sut = new ConfigPlansProvider(Enumerable.Empty<Plan>());

        var plan = sut.GetFreePlan();

        plan.Should().NotBeNull();
    }

    [Fact]
    public void Should_return_fitting_app_plan()
    {
        var sut = new ConfigPlansProvider(Plans);

        var actual = sut.GetActualPlan("basic");

        actual.Should().BeEquivalentTo((BasicPlan, "basic"));
    }

    [Fact]
    public void Should_return_fitting_yearly_app_plan()
    {
        var sut = new ConfigPlansProvider(Plans);

        var actual = sut.GetActualPlan("basic_yearly");

        actual.Should().BeEquivalentTo((BasicPlan, "basic_yearly"));
    }

    [Fact]
    public void Should_smallest_plan_if_none_fits()
    {
        var sut = new ConfigPlansProvider(Plans);

        var actual = sut.GetActualPlan("enterprise");

        actual.Should().BeEquivalentTo((FreePlan, "free"));
    }

    [Fact]
    public void Should_check_plan_exists()
    {
        var sut = new ConfigPlansProvider(Plans);

        Assert.True(sut.IsConfiguredPlan("basic"));
        Assert.True(sut.IsConfiguredPlan("free"));

        Assert.False(sut.IsConfiguredPlan("infinite"));
        Assert.False(sut.IsConfiguredPlan("invalid"));
        Assert.False(sut.IsConfiguredPlan(null));
    }
}
