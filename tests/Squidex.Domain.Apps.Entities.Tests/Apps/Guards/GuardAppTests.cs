// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Infrastructure;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Guards
{
    public class GuardAppTests
    {
        private readonly IAppProvider apps = A.Fake<IAppProvider>();
        private readonly IUserResolver users = A.Fake<IUserResolver>();
        private readonly IAppPlansProvider appPlans = A.Fake<IAppPlansProvider>();

        public GuardAppTests()
        {
            A.CallTo(() => apps.GetAppAsync("new-app"))
                .Returns(Task.FromResult<IAppEntity>(null));

            A.CallTo(() => users.FindByIdAsync(A<string>.Ignored))
                .Returns(A.Fake<IUser>());

            A.CallTo(() => appPlans.GetPlan("free"))
                .Returns(A.Fake<IAppLimitsPlan>());
        }

        [Fact]
        public Task CanCreate_should_throw_exception_if_name_already_in_use()
        {
            A.CallTo(() => apps.GetAppAsync("new-app"))
                .Returns(A.Fake<IAppEntity>());

            var command = new CreateApp { Name = "new-app" };

            return Assert.ThrowsAsync<ValidationException>(() => GuardApp.CanCreate(command, apps));
        }

        [Fact]
        public Task CanCreate_should_throw_exception_if_name_not_valid()
        {
            var command = new CreateApp { Name = "INVALID NAME" };

            return Assert.ThrowsAsync<ValidationException>(() => GuardApp.CanCreate(command, apps));
        }

        [Fact]
        public Task CanCreate_should_not_throw_exception_if_app_name_is_free()
        {
            var command = new CreateApp { Name = "new-app" };

            return GuardApp.CanCreate(command, apps);
        }

        [Fact]
        public void CanChangePlan_should_throw_exception_if_plan_id_null()
        {
            var command = new ChangePlan { Actor = new RefToken("user", "me") };

            AppPlan plan = null;

            Assert.Throws<ValidationException>(() => GuardApp.CanChangePlan(command, plan, appPlans));
        }

        [Fact]
        public void CanChangePlan_should_throw_exception_if_plan_not_found()
        {
            A.CallTo(() => appPlans.GetPlan("free"))
                .Returns(null);

            var command = new ChangePlan { PlanId = "free", Actor = new RefToken("user", "me") };

            AppPlan plan = null;

            Assert.Throws<ValidationException>(() => GuardApp.CanChangePlan(command, plan, appPlans));
        }

        [Fact]
        public void CanChangePlan_should_throw_exception_if_plan_was_configured_from_another_user()
        {
            var command = new ChangePlan { PlanId = "free", Actor = new RefToken("user", "me") };

            var plan = new AppPlan(new RefToken("user", "other"), "premium");

            Assert.Throws<ValidationException>(() => GuardApp.CanChangePlan(command, plan, appPlans));
        }

        [Fact]
        public void CanChangePlan_should_throw_exception_if_plan_is_the_same()
        {
            var command = new ChangePlan { PlanId = "free", Actor = new RefToken("user", "me") };

            var plan = new AppPlan(new RefToken("user", "me"), "free");

            Assert.Throws<ValidationException>(() => GuardApp.CanChangePlan(command, plan, appPlans));
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
