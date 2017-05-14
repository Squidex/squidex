// ==========================================================================
//  ConfigAppLimitsProviderTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using FluentAssertions;
using Moq;
using Squidex.Read.Apps.Services.Implementations;
using Xunit;

namespace Squidex.Read.Apps
{
    public class ConfigAppLimitsProviderTests
    {
        private static readonly ConfigAppLimitsPlan[] Plans =
        {
            new ConfigAppLimitsPlan
            {
                Name = "Basic",
                MaxApiCalls = 150000,
                MaxAssetSize = 1024 * 1024 * 2,
                MaxContributors = 5
            },
            new ConfigAppLimitsPlan
            {
                Name = "Free",
                MaxApiCalls = 50000,
                MaxAssetSize = 1024 * 1024 * 10,
                MaxContributors = 2
            }
        };

        [Fact]
        public void Should_return_plans()
        {
            var sut = new ConfigAppLimitsProvider(Plans);

            Plans.OrderBy(x => x.MaxApiCalls).ShouldBeEquivalentTo(sut.GetAvailablePlans());
        }

        [Fact]
        public void Should_return_infinite_if_nothing_configured()
        {
            var sut = new ConfigAppLimitsProvider(Enumerable.Empty<ConfigAppLimitsPlan>());

            var plan = sut.GetPlanForApp(CreateApp(0));

            plan.ShouldBeEquivalentTo(new ConfigAppLimitsPlan
            {
                Name = "Infinite",
                MaxApiCalls = -1,
                MaxAssetSize = -1,
                MaxContributors = -1
            });
        }

        [Fact]
        public void Should_return_fitting_app_plan()
        {
            var sut = new ConfigAppLimitsProvider(Plans);

            var plan = sut.GetPlanForApp(CreateApp(1));

            plan.ShouldBeEquivalentTo(new ConfigAppLimitsPlan
            {
                Name = "Basic",
                MaxApiCalls = 150000,
                MaxAssetSize = 1024 * 1024 * 2,
                MaxContributors = 5
            });
        }

        [Fact]
        public void Should_smallest_plan_if_none_fits()
        {
            var sut = new ConfigAppLimitsProvider(Plans);

            var plan = sut.GetPlanForApp(CreateApp(4));

            plan.ShouldBeEquivalentTo(new ConfigAppLimitsPlan
            {
                Name = "Free",
                MaxApiCalls = 50000,
                MaxAssetSize = 1024 * 1024 * 10,
                MaxContributors = 2
            });
        }

        private static IAppEntity CreateApp(int plan)
        {
            var app = new Mock<IAppEntity>();

            app.Setup(x => x.PlanId).Returns(plan);

            return app.Object;
        }
    }
}
