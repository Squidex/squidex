// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Plans;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;
using Squidex.Shared.Users;
using Xunit;

#pragma warning disable SA1310 // Field names must not contain underscore

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject.Guards
{
    public class GuardAppContributorsTests : IClassFixture<TranslationsFixture>
    {
        private readonly IUser user1 = UserMocks.User("1");
        private readonly IUser user2 = UserMocks.User("2");
        private readonly IUser user3 = UserMocks.User("3");
        private readonly IUserResolver users = A.Fake<IUserResolver>();
        private readonly IAppLimitsPlan appPlan = A.Fake<IAppLimitsPlan>();
        private readonly AppContributors contributors_0 = AppContributors.Empty;
        private readonly Roles roles = Roles.Empty;

        public GuardAppContributorsTests()
        {
            A.CallTo(() => user1.Id)
                .Returns("1");

            A.CallTo(() => user2.Id)
                .Returns("2");

            A.CallTo(() => user3.Id)
                .Returns("3");

            A.CallTo(() => users.FindByIdAsync("1", default))
                .Returns(user1);

            A.CallTo(() => users.FindByIdAsync("2", default))
                .Returns(user2);

            A.CallTo(() => users.FindByIdAsync("3", default))
                .Returns(user3);

            A.CallTo(() => users.FindByIdAsync("notfound", default))
                .Returns(Task.FromResult<IUser?>(null));

            A.CallTo(() => appPlan.MaxContributors)
                .Returns(10);
        }

        [Fact]
        public async Task CanAssign_should_throw_exception_if_contributor_id_is_null()
        {
            var command = new AssignContributor();

            await ValidationAssert.ThrowsAsync(() => GuardAppContributors.CanAssign(command, App(contributors_0), users, appPlan),
                new ValidationError("Contributor ID or email is required.", "ContributorId"));
        }

        [Fact]
        public async Task CanAssign_should_throw_exception_if_role_not_valid()
        {
            var command = new AssignContributor { ContributorId = "1", Role = "Invalid" };

            await ValidationAssert.ThrowsAsync(() => GuardAppContributors.CanAssign(command, App(contributors_0), users, appPlan),
                new ValidationError("Role is not a valid value.", "Role"));
        }

        [Fact]
        public async Task CanAssign_should_not_throw_exception_if_user_already_exists_with_same_role()
        {
            var command = new AssignContributor { ContributorId = "1", Role = Role.Owner };

            var contributors_1 = contributors_0.Assign("1", Role.Owner);

            await GuardAppContributors.CanAssign(command, App(contributors_1), users, appPlan);
        }

        [Fact]
        public async Task CanAssign_should_not_throw_exception_if_user_already_exists_with_some_role_but_is_from_restore()
        {
            var command = new AssignContributor { ContributorId = "1", Role = Role.Owner, Restoring = true };

            var contributors_1 = contributors_0.Assign("1", Role.Owner);

            await GuardAppContributors.CanAssign(command, App(contributors_1), users, appPlan);
        }

        [Fact]
        public async Task CanAssign_should_throw_exception_if_user_not_found()
        {
            var command = new AssignContributor { ContributorId = "notfound", Role = Role.Owner };

            await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => GuardAppContributors.CanAssign(command, App(contributors_0), users, appPlan));
        }

        [Fact]
        public async Task CanAssign_should_throw_exception_if_user_is_actor()
        {
            var command = new AssignContributor { ContributorId = "3", Role = Role.Editor, Actor = RefToken.User("3") };

            await Assert.ThrowsAsync<DomainForbiddenException>(() => GuardAppContributors.CanAssign(command, App(contributors_0), users, appPlan));
        }

        [Fact]
        public async Task CanAssign_should_throw_exception_if_contributor_max_reached()
        {
            A.CallTo(() => appPlan.MaxContributors)
                .Returns(2);

            var command = new AssignContributor { ContributorId = "3" };

            var contributors_1 = contributors_0.Assign("1", Role.Owner);
            var contributors_2 = contributors_1.Assign("2", Role.Editor);

            await ValidationAssert.ThrowsAsync(() => GuardAppContributors.CanAssign(command, App(contributors_2), users, appPlan),
                new ValidationError("You have reached the maximum number of contributors for your plan."));
        }

        [Fact]
        public async Task CanAssign_should_not_throw_exception_if_user_found()
        {
            A.CallTo(() => appPlan.MaxContributors)
                .Returns(-1);

            var command = new AssignContributor { ContributorId = "1" };

            await GuardAppContributors.CanAssign(command, App(contributors_0), users, appPlan);
        }

        [Fact]
        public async Task CanAssign_should_not_throw_exception_if_contributor_has_another_role()
        {
            var command = new AssignContributor { ContributorId = "1" };

            var contributors_1 = contributors_0.Assign("1", Role.Developer);

            await GuardAppContributors.CanAssign(command, App(contributors_1), users, appPlan);
        }

        [Fact]
        public async Task CanAssign_should_not_throw_exception_if_contributor_max_reached_but_role_changed()
        {
            A.CallTo(() => appPlan.MaxContributors)
                .Returns(2);

            var command = new AssignContributor { ContributorId = "1" };

            var contributors_1 = contributors_0.Assign("1", Role.Developer);
            var contributors_2 = contributors_1.Assign("2", Role.Developer);

            await GuardAppContributors.CanAssign(command, App(contributors_2), users, appPlan);
        }

        [Fact]
        public async Task CanAssign_should_not_throw_exception_if_contributor_max_reached_but_from_restore()
        {
            A.CallTo(() => appPlan.MaxContributors)
                .Returns(2);

            var command = new AssignContributor { ContributorId = "3", Restoring = true };

            var contributors_1 = contributors_0.Assign("1", Role.Editor);
            var contributors_2 = contributors_1.Assign("2", Role.Editor);

            await GuardAppContributors.CanAssign(command, App(contributors_2), users, appPlan);
        }

        [Fact]
        public void CanRemove_should_throw_exception_if_contributor_id_is_null()
        {
            var command = new RemoveContributor();

            ValidationAssert.Throws(() => GuardAppContributors.CanRemove(command, App(contributors_0)),
                new ValidationError("Contributor ID or email is required.", "ContributorId"));
        }

        [Fact]
        public void CanRemove_should_throw_exception_if_contributor_not_found()
        {
            var command = new RemoveContributor { ContributorId = "1" };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardAppContributors.CanRemove(command, App(contributors_0)));
        }

        [Fact]
        public void CanRemove_should_throw_exception_if_contributor_is_only_owner()
        {
            var command = new RemoveContributor { ContributorId = "1" };

            var contributors_1 = contributors_0.Assign("1", Role.Owner);
            var contributors_2 = contributors_1.Assign("2", Role.Editor);

            ValidationAssert.Throws(() => GuardAppContributors.CanRemove(command, App(contributors_2)),
                new ValidationError("Cannot remove the only owner."));
        }

        [Fact]
        public void CanRemove_should_not_throw_exception_if_contributor_not_only_owner()
        {
            var command = new RemoveContributor { ContributorId = "1" };

            var contributors_1 = contributors_0.Assign("1", Role.Owner);
            var contributors_2 = contributors_1.Assign("2", Role.Owner);

            GuardAppContributors.CanRemove(command, App(contributors_2));
        }

        private IAppEntity App(AppContributors contributors)
        {
            var app = A.Fake<IAppEntity>();

            A.CallTo(() => app.Contributors)
                .Returns(contributors);
            A.CallTo(() => app.Roles)
                .Returns(roles);

            return app;
        }
    }
}
