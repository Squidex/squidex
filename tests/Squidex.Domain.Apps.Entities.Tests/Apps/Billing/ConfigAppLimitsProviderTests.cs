// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using FakeItEasy;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Services.Implementations;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Billing
{
    public class ConfigAppLimitsProviderTests
    {
        private static readonly ConfigAppLimitsPlan InfinitePlan = new ConfigAppLimitsPlan
        {
            Id = "infinite",
            Name = "Infinite",
            MaxApiCalls = -1,
            MaxAssetSize = -1,
            MaxContributors = -1
        };

        private static readonly ConfigAppLimitsPlan FreePlan = new ConfigAppLimitsPlan
        {
            Id = "free",
            Name = "Free",
            MaxApiCalls = 50000,
            MaxAssetSize = 1024 * 1024 * 10,
            MaxContributors = 2
        };

        private static readonly ConfigAppLimitsPlan BasicPlan = new ConfigAppLimitsPlan
        {
            Id = "basic",
            Name = "Basic",
            MaxApiCalls = 150000,
            MaxAssetSize = 1024 * 1024 * 2,
            MaxContributors = 5
        };

        private static readonly ConfigAppLimitsPlan[] Plans = { BasicPlan, FreePlan };

        [Fact]
        public void Should_return_plans()
        {
            var sut = new ConfigAppPlansProvider(Plans);

            Plans.OrderBy(x => x.MaxApiCalls).ShouldBeEquivalentTo(sut.GetAvailablePlans());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("my-plan")]
        public void Should_return_infinite_if_nothing_configured(string planId)
        {
            var sut = new ConfigAppPlansProvider(Enumerable.Empty<ConfigAppLimitsPlan>());

            var plan = sut.GetPlanForApp(CreateApp(planId));

            plan.ShouldBeEquivalentTo(InfinitePlan);
        }

        [Fact]
        public void Should_return_fitting_app_plan()
        {
            var sut = new ConfigAppPlansProvider(Plans);

            var plan = sut.GetPlanForApp(CreateApp("basic"));

            plan.ShouldBeEquivalentTo(BasicPlan);
        }

        [Fact]
        public void Should_smallest_plan_if_none_fits()
        {
            var sut = new ConfigAppPlansProvider(Plans);

            var plan = sut.GetPlanForApp(CreateApp("enterprise"));

            plan.ShouldBeEquivalentTo(FreePlan);
        }

        [Fact]
        public void Should_return_second_plan_for_upgrade_if_plan_is_null()
        {
            var sut = new ConfigAppPlansProvider(Plans);

            var upgradePlan = sut.GetPlanUpgrade(null);

            upgradePlan.ShouldBeEquivalentTo(BasicPlan);
        }

        [Fact]
        public void Should_return_second_plan_for_upgrade_if_plan_not_found()
        {
            var sut = new ConfigAppPlansProvider(Plans);

            var upgradePlan = sut.GetPlanUpgradeForApp(CreateApp("enterprise"));

            upgradePlan.ShouldBeEquivalentTo(BasicPlan);
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

            upgradePlan.ShouldBeEquivalentTo(BasicPlan);
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
                A.CallTo(() => app.Plan).Returns(new AppPlan(new RefToken("user", "me"), plan));
            }
            else
            {
                A.CallTo(() => app.Plan).Returns(null);
            }

            return app;
        }
    }
}
