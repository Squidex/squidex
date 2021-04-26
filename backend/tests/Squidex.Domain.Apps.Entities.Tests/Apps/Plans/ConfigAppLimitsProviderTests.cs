// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using FakeItEasy;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Plans
{
    public class ConfigAppLimitsProviderTests
    {
        private static readonly ConfigAppLimitsPlan InfinitePlan = new ConfigAppLimitsPlan
        {
            Id = "infinite",
            Name = "Infinite",
            MaxApiCalls = -1,
            MaxAssetSize = -1,
            MaxContributors = -1,
            BlockingApiCalls = -1
        };

        private static readonly ConfigAppLimitsPlan FreePlan = new ConfigAppLimitsPlan
        {
            Id = "free",
            Name = "Free",
            MaxApiCalls = 50000,
            MaxAssetSize = 1024 * 1024 * 10,
            MaxContributors = 2,
            BlockingApiCalls = 50000,
            IsFree = true
        };

        private static readonly ConfigAppLimitsPlan BasicPlan = new ConfigAppLimitsPlan
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

        private static readonly ConfigAppLimitsPlan[] Plans = { BasicPlan, FreePlan };

        [Fact]
        public void Should_return_plans()
        {
            var sut = new ConfigAppPlansProvider(Plans);

            Plans.OrderBy(x => x.MaxApiCalls).Should().BeEquivalentTo(sut.GetAvailablePlans());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("my-plan")]
        public void Should_return_infinite_if_nothing_configured(string planId)
        {
            var sut = new ConfigAppPlansProvider(Enumerable.Empty<ConfigAppLimitsPlan>());

            var result = sut.GetPlanForApp(CreateApp(planId));

            result.Should().BeEquivalentTo((InfinitePlan, "infinite"));
        }

        [Fact]
        public void Should_return_free_plan()
        {
            var sut = new ConfigAppPlansProvider(Plans);

            var plan = sut.GetFreePlan();

            plan.Should().BeEquivalentTo(FreePlan);
        }

        [Fact]
        public void Should_return_infinite_plan_for_free_plan_if_not_found()
        {
            var sut = new ConfigAppPlansProvider(Enumerable.Empty<ConfigAppLimitsPlan>());

            var plan = sut.GetFreePlan();

            plan.Should().NotBeNull();
        }

        [Fact]
        public void Should_return_fitting_app_plan()
        {
            var sut = new ConfigAppPlansProvider(Plans);

            var result = sut.GetPlanForApp(CreateApp("basic"));

            result.Should().BeEquivalentTo((BasicPlan, "basic"));
        }

        [Fact]
        public void Should_return_fitting_yearly_app_plan()
        {
            var sut = new ConfigAppPlansProvider(Plans);

            var result = sut.GetPlanForApp(CreateApp("basic_yearly"));

            result.Should().BeEquivalentTo((BasicPlan, "basic_yearly"));
        }

        [Fact]
        public void Should_smallest_plan_if_none_fits()
        {
            var sut = new ConfigAppPlansProvider(Plans);

            var result = sut.GetPlanForApp(CreateApp("enterprise"));

            result.Should().BeEquivalentTo((FreePlan, "free"));
        }

        [Fact]
        public void Should_return_second_plan_for_upgrade_if_plan_is_null()
        {
            var sut = new ConfigAppPlansProvider(Plans);

            var upgradePlan = sut.GetPlanUpgrade(null);

            upgradePlan.Should().BeEquivalentTo(BasicPlan);
        }

        [Fact]
        public void Should_return_second_plan_for_upgrade_if_plan_not_found()
        {
            var sut = new ConfigAppPlansProvider(Plans);

            var upgradePlan = sut.GetPlanUpgradeForApp(CreateApp("enterprise"));

            upgradePlan.Should().BeEquivalentTo(BasicPlan);
        }

        [Fact]
        public void Should_not_return_plan_for_upgrade_if_plan_is_highest_plan()
        {
            var sut = new ConfigAppPlansProvider(Plans);

            var upgradePlan = sut.GetPlanUpgradeForApp(CreateApp("basic"));

            Assert.Null(upgradePlan);
        }

        [Fact]
        public void Should_return_next_plan_if_plan_is_upgradeable()
        {
            var sut = new ConfigAppPlansProvider(Plans);

            var upgradePlan = sut.GetPlanUpgradeForApp(CreateApp("free"));

            upgradePlan.Should().BeEquivalentTo(BasicPlan);
        }

        [Fact]
        public void Should_check_plan_exists()
        {
            var sut = new ConfigAppPlansProvider(Plans);

            Assert.True(sut.IsConfiguredPlan("basic"));
            Assert.True(sut.IsConfiguredPlan("free"));

            Assert.False(sut.IsConfiguredPlan("infinite"));
            Assert.False(sut.IsConfiguredPlan("invalid"));
            Assert.False(sut.IsConfiguredPlan(null));
        }

        private static IAppEntity CreateApp(string plan)
        {
            var app = A.Dummy<IAppEntity>();

            if (plan != null)
            {
                A.CallTo(() => app.Plan)
                    .Returns(new AppPlan(RefToken.User("me"), plan));
            }
            else
            {
                A.CallTo(() => app.Plan)
                    .Returns(null);
            }

            return app;
        }
    }
}
