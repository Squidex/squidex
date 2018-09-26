// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Guards
{
    public class GuardAppTests
    {
        private readonly IUserResolver users = A.Fake<IUserResolver>();
        private readonly IAppPlansProvider appPlans = A.Fake<IAppPlansProvider>();

        public GuardAppTests()
        {
            A.CallTo(() => users.FindByIdOrEmailAsync(A<string>.Ignored))
                .Returns(A.Dummy<IUser>());

            A.CallTo(() => appPlans.GetPlan("notfound"))
                .Returns(null);

            A.CallTo(() => appPlans.GetPlan("free"))
                .Returns(A.Dummy<IAppLimitsPlan>());
        }

        [Fact]
        public void CanCreate_should_throw_exception_if_name_not_valid()
        {
            var command = new CreateApp { Name = "INVALID NAME" };

            ValidationAssert.Throws(() => GuardApp.CanCreate(command),
                new ValidationError("Name must be a valid slug.", "Name"));
        }

        [Fact]
        public void CanCreate_should_not_throw_exception_if_app_name_is_valid()
        {
            var command = new CreateApp { Name = "new-app" };

            GuardApp.CanCreate(command);
        }

        [Fact]
        public void CanChangePlan_should_throw_exception_if_plan_id_is_null()
        {
            var command = new ChangePlan { Actor = new RefToken("user", "me") };

            AppPlan plan = null;

            ValidationAssert.Throws(() => GuardApp.CanChangePlan(command, plan, appPlans),
                new ValidationError("Plan id is required.", "PlanId"));
        }

        [Fact]
        public void CanChangePlan_should_throw_exception_if_plan_not_found()
        {
            var command = new ChangePlan { PlanId = "notfound", Actor = new RefToken("user", "me") };

            AppPlan plan = null;

            ValidationAssert.Throws(() => GuardApp.CanChangePlan(command, plan, appPlans),
                new ValidationError("A plan with this id does not exist.", "PlanId"));
        }

        [Fact]
        public void CanChangePlan_should_throw_exception_if_plan_was_configured_from_another_user()
        {
            var command = new ChangePlan { PlanId = "free", Actor = new RefToken("user", "me") };

            var plan = new AppPlan(new RefToken("user", "other"), "premium");

            ValidationAssert.Throws(() => GuardApp.CanChangePlan(command, plan, appPlans),
                new ValidationError("Plan can only changed from the user who configured the plan initially."));
        }

        [Fact]
        public void CanChangePlan_should_throw_exception_if_plan_is_the_same()
        {
            var command = new ChangePlan { PlanId = "free", Actor = new RefToken("user", "me") };

            var plan = new AppPlan(new RefToken("user", "me"), "free");

            ValidationAssert.Throws(() => GuardApp.CanChangePlan(command, plan, appPlans),
                new ValidationError("App has already this plan."));
        }

        [Fact]
        public void CanChangePlan_should_not_throw_exception_if_same_user_but_other_plan()
        {
            var command = new ChangePlan { PlanId = "free", Actor = new RefToken("user", "me") };

            var plan = new AppPlan(new RefToken("user", "me"), "premium");

            GuardApp.CanChangePlan(command, plan, appPlans);
        }
    }
}
