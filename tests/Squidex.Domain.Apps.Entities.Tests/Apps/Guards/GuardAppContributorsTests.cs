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

#pragma warning disable SA1310 // Field names must not contain underscore

namespace Squidex.Domain.Apps.Entities.Apps.Guards
{
    public class GuardAppContributorsTests
    {
        private readonly IUserResolver users = A.Fake<IUserResolver>();
        private readonly IAppLimitsPlan appPlan = A.Fake<IAppLimitsPlan>();
        private readonly AppContributors contributors_0 = AppContributors.Empty;

        public GuardAppContributorsTests()
        {
            A.CallTo(() => users.FindByIdAsync(A<string>.Ignored))
                .Returns(A.Fake<IUser>());

            A.CallTo(() => appPlan.MaxContributors)
                .Returns(10);
        }

        [Fact]
        public Task CanAssign_should_throw_exception_if_contributor_id_is_null()
        {
            var command = new AssignContributor();

            return Assert.ThrowsAsync<ValidationException>(() => GuardAppContributors.CanAssign(contributors_0, command, users, appPlan));
        }

        [Fact]
        public Task CanAssign_should_throw_exception_if_permission_not_valid()
        {
            var command = new AssignContributor { ContributorId = "1", Permission = (AppContributorPermission)10 };

            return Assert.ThrowsAsync<ValidationException>(() => GuardAppContributors.CanAssign(contributors_0, command, users, appPlan));
        }

        [Fact]
        public Task CanAssign_should_throw_exception_if_user_already_exists_with_same_permission()
        {
            var command = new AssignContributor { ContributorId = "1" };

            var contributors_1 = contributors_0.Assign("1", AppContributorPermission.Owner);

            return Assert.ThrowsAsync<ValidationException>(() => GuardAppContributors.CanAssign(contributors_1, command, users, appPlan));
        }

        [Fact]
        public Task CanAssign_should_throw_exception_if_user_not_found()
        {
            A.CallTo(() => users.FindByIdAsync(A<string>.Ignored))
                .Returns(Task.FromResult<IUser>(null));

            var command = new AssignContributor { ContributorId = "1", Permission = (AppContributorPermission)10 };

            return Assert.ThrowsAsync<ValidationException>(() => GuardAppContributors.CanAssign(contributors_0, command, users, appPlan));
        }

        [Fact]
        public Task CanAssign_should_throw_exception_if_contributor_max_reached()
        {
            A.CallTo(() => appPlan.MaxContributors)
                .Returns(2);

            var command = new AssignContributor { ContributorId = "3" };

            var contributors_1 = contributors_0.Assign("1", AppContributorPermission.Owner);
            var contributors_2 = contributors_1.Assign("2", AppContributorPermission.Editor);

            return Assert.ThrowsAsync<ValidationException>(() => GuardAppContributors.CanAssign(contributors_2, command, users, appPlan));
        }

        [Fact]
        public Task CanAssign_should_not_throw_exception_if_user_found()
        {
            var command = new AssignContributor { ContributorId = "1" };

            return GuardAppContributors.CanAssign(contributors_0, command, users, appPlan);
        }

        [Fact]
        public Task CanAssign_should_not_throw_exception_if_contributor_has_another_permission()
        {
            var command = new AssignContributor { ContributorId = "1" };

            var contributors_1 = contributors_0.Assign("1", AppContributorPermission.Editor);

            return GuardAppContributors.CanAssign(contributors_1, command, users, appPlan);
        }

        [Fact]
        public Task CanAssign_should_not_throw_exception_if_contributor_max_reached_but_permission_changed()
        {
            A.CallTo(() => appPlan.MaxContributors)
                .Returns(2);

            var command = new AssignContributor { ContributorId = "1" };

            var contributors_1 = contributors_0.Assign("1", AppContributorPermission.Editor);
            var contributors_2 = contributors_1.Assign("2", AppContributorPermission.Editor);

            return GuardAppContributors.CanAssign(contributors_2, command, users, appPlan);
        }

        [Fact]
        public void CanRemove_should_throw_exception_if_contributor_id_is_null()
        {
            var command = new RemoveContributor();

            Assert.Throws<ValidationException>(() => GuardAppContributors.CanRemove(contributors_0, command));
        }

        [Fact]
        public void CanRemove_should_throw_exception_if_contributor_not_found()
        {
            var command = new RemoveContributor { ContributorId = "1" };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardAppContributors.CanRemove(contributors_0, command));
        }

        [Fact]
        public void CanRemove_should_throw_exception_if_contributor_is_only_owner()
        {
            var command = new RemoveContributor { ContributorId = "1" };

            var contributors_1 = contributors_0.Assign("1", AppContributorPermission.Owner);
            var contributors_2 = contributors_1.Assign("2", AppContributorPermission.Editor);

            Assert.Throws<ValidationException>(() => GuardAppContributors.CanRemove(contributors_2, command));
        }

        [Fact]
        public void CanRemove_should_not_throw_exception_if_contributor_not_only_owner()
        {
            var command = new RemoveContributor { ContributorId = "1" };

            var contributors_1 = contributors_0.Assign("1", AppContributorPermission.Owner);
            var contributors_2 = contributors_1.Assign("2", AppContributorPermission.Owner);

            GuardAppContributors.CanRemove(contributors_2, command);
        }
    }
}
