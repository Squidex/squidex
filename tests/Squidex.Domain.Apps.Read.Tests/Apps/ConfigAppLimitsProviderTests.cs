// ==========================================================================
//  ConfigAppLimitsProviderTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using FakeItEasy;
using FluentAssertions;
using Squidex.Domain.Apps.Read.Apps.Services.Implementations;
using Xunit;

namespace Squidex.Domain.Apps.Read.Apps
{
    public class ConfigAppLimitsProviderTests
    {
        private static readonly ConfigAppLimitsPlan[] Plans =
        {
            new ConfigAppLimitsPlan
            {
                Id = "basic",
                Name = "Basic",
                MaxApiCalls = 150000,
                MaxAssetSize = 1024 * 1024 * 2,
                MaxContributors = 5
            },
            new ConfigAppLimitsPlan
            {
                Id = "free",
                Name = "Free",
                MaxApiCalls = 50000,
                MaxAssetSize = 1024 * 1024 * 10,
                MaxContributors = 2
            }
        };

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

            plan.ShouldBeEquivalentTo(new ConfigAppLimitsPlan
            {
                Id = "infinite",
                Name = "Infinite",
                MaxApiCalls = -1,
                MaxAssetSize = -1,
                MaxContributors = -1
            });
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

        [Fact]
        public void Should_return_fitting_app_plan()
        {
            var sut = new ConfigAppPlansProvider(Plans);

            var plan = sut.GetPlanForApp(CreateApp("basic"));

            plan.ShouldBeEquivalentTo(new ConfigAppLimitsPlan
            {
                Id = "basic",
                Name = "Basic",
                MaxApiCalls = 150000,
                MaxAssetSize = 1024 * 1024 * 2,
                MaxContributors = 5
            });
        }

        [Fact]
        public void Should_smallest_plan_if_none_fits()
        {
            var sut = new ConfigAppPlansProvider(Plans);

            var plan = sut.GetPlanForApp(CreateApp("Enterprise"));

            plan.ShouldBeEquivalentTo(new ConfigAppLimitsPlan
            {
                Id = "free",
                Name = "Free",
                MaxApiCalls = 50000,
                MaxAssetSize = 1024 * 1024 * 10,
                MaxContributors = 2
            });
        }

        private static IAppEntity CreateApp(string plan)
        {
            var app = A.Dummy<IAppEntity>();

            A.CallTo(() => app.PlanId).Returns(plan);

            return app;
        }
    }
}
