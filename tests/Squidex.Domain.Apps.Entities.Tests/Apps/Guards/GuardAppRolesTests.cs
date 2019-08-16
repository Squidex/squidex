// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Xunit;

#pragma warning disable SA1310 // Field names must not contain underscore

namespace Squidex.Domain.Apps.Entities.Apps.Guards
{
    public class GuardAppRolesTests
    {
        private readonly string roleName = "Role1";
        private readonly Roles roles_0 = Roles.Empty;
        private readonly AppContributors contributors = AppContributors.Empty;
        private readonly AppClients clients = AppClients.Empty;

        [Fact]
        public void CanAdd_should_throw_exception_if_name_empty()
        {
            var command = new AddRole { Name = null };

            ValidationAssert.Throws(() => GuardAppRoles.CanAdd(roles_0, command),
                new ValidationError("Name is required.", "Name"));
        }

        [Fact]
        public void CanAdd_should_throw_exception_if_name_exists()
        {
            var roles_1 = roles_0.Add(roleName);

            var command = new AddRole { Name = roleName };

            ValidationAssert.Throws(() => GuardAppRoles.CanAdd(roles_1, command),
                new ValidationError("A role with the same name already exists."));
        }

        [Fact]
        public void CanAdd_should_not_throw_exception_if_command_is_valid()
        {
            var command = new AddRole { Name = roleName };

            GuardAppRoles.CanAdd(roles_0, command);
        }

        [Fact]
        public void CanDelete_should_throw_exception_if_name_empty()
        {
            var command = new DeleteRole { Name = null };

            ValidationAssert.Throws(() => GuardAppRoles.CanDelete(roles_0, command, contributors, clients),
                new ValidationError("Name is required.", "Name"));
        }

        [Fact]
        public void CanDelete_should_throw_exception_if_role_not_found()
        {
            var command = new DeleteRole { Name = roleName };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardAppRoles.CanDelete(roles_0, command, contributors, clients));
        }

        [Fact]
        public void CanDelete_should_throw_exception_if_contributor_found()
        {
            var roles_1 = roles_0.Add(roleName);

            var command = new DeleteRole { Name = roleName };

            ValidationAssert.Throws(() => GuardAppRoles.CanDelete(roles_1, command, contributors.Assign("1", roleName), clients),
                new ValidationError("Cannot remove a role when a contributor is assigned."));
        }

        [Fact]
        public void CanDelete_should_throw_exception_if_client_found()
        {
            var roles_1 = roles_0.Add(roleName);

            var command = new DeleteRole { Name = roleName };

            ValidationAssert.Throws(() => GuardAppRoles.CanDelete(roles_1, command, contributors, clients.Add("1", new AppClient("client", "1", roleName))),
                new ValidationError("Cannot remove a role when a client is assigned."));
        }

        [Fact]
        public void CanDelete_should_throw_exception_if_default_role()
        {
            var roles_1 = roles_0.Add(Role.Developer);

            var command = new DeleteRole { Name = Role.Developer };

            ValidationAssert.Throws(() => GuardAppRoles.CanDelete(roles_1, command, contributors, clients),
                new ValidationError("Cannot delete a default role."));
        }

        [Fact]
        public void CanDelete_should_not_throw_exception_if_command_is_valid()
        {
            var roles_1 = roles_0.Add(roleName);

            var command = new DeleteRole { Name = roleName };

            GuardAppRoles.CanDelete(roles_1, command, contributors, clients);
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_name_empty()
        {
            var roles_1 = roles_0.Add(roleName);

            var command = new UpdateRole { Name = null, Permissions = new[] { "P1" } };

            ValidationAssert.Throws(() => GuardAppRoles.CanUpdate(roles_1, command),
                new ValidationError("Name is required.", "Name"));
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_permission_is_null()
        {
            var roles_1 = roles_0.Add(roleName);

            var command = new UpdateRole { Name = roleName, Permissions = null };

            ValidationAssert.Throws(() => GuardAppRoles.CanUpdate(roles_1, command),
                new ValidationError("Permissions is required.", "Permissions"));
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_default_role()
        {
            var roles_1 = roles_0.Add(Role.Developer);

            var command = new UpdateRole { Name = Role.Developer, Permissions = new[] { "P1" } };

            ValidationAssert.Throws(() => GuardAppRoles.CanUpdate(roles_1, command),
                new ValidationError("Cannot update a default role."));
        }

        [Fact]
        public void CanUpdate_should_throw_exception_if_role_does_not_exists()
        {
            var command = new UpdateRole { Name = roleName, Permissions = new[] { "P1" } };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardAppRoles.CanUpdate(roles_0, command));
        }

        [Fact]
        public void CanUpdate_should_not_throw_exception_if_role_exist_with_valid_command()
        {
            var roles_1 = roles_0.Add(roleName);

            var command = new UpdateRole { Name = roleName, Permissions = new[] { "P1" } };

            GuardAppRoles.CanUpdate(roles_1, command);
        }
    }
}
