// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject.Guards;

public class GuardAppRolesTests : GivenContext, IClassFixture<TranslationsFixture>
{
    private readonly string roleName = "Role1";

    public GuardAppRolesTests()
    {
        App = App with
        {
            Contributors = Contributors.Empty.Assign(User.Identifier, "contributorRole")
        };

        App = App with
        {
            Clients = AppClients.Empty.Add(Client.Identifier, "secret", "clientRole")
        };
    }

    [Fact]
    public void CanAdd_should_throw_exception_if_name_empty()
    {
        var command = new AddRole { Name = null! };

        ValidationAssert.Throws(() => GuardAppRoles.CanAdd(command, App),
            new ValidationError("Name is required.", "Name"));
    }

    [Fact]
    public void CanAdd_should_throw_exception_if_name_exists()
    {
        App = App with
        {
            Roles = Roles.Empty.Add(roleName)
        };

        var command = new AddRole { Name = roleName };

        ValidationAssert.Throws(() => GuardAppRoles.CanAdd(command, App),
            new ValidationError("A role with the same name already exists."));
    }

    [Fact]
    public void CanAdd_should_not_throw_exception_if_command_is_valid()
    {
        var command = new AddRole { Name = roleName };

        GuardAppRoles.CanAdd(command, App);
    }

    [Fact]
    public void CanDelete_should_throw_exception_if_name_empty()
    {
        var command = new DeleteRole { Name = null! };

        ValidationAssert.Throws(() => GuardAppRoles.CanDelete(command, App),
            new ValidationError("Name is required.", "Name"));
    }

    [Fact]
    public void CanDelete_should_throw_exception_if_role_not_found()
    {
        var command = new DeleteRole { Name = roleName };

        Assert.Throws<DomainObjectNotFoundException>(() => GuardAppRoles.CanDelete(command, App));
    }

    [Fact]
    public void CanDelete_should_throw_exception_if_contributor_found()
    {
        App = App with
        {
            Roles = Roles.Empty.Add("contributorRole")
        };

        var command = new DeleteRole { Name = "contributorRole" };

        ValidationAssert.Throws(() => GuardAppRoles.CanDelete(command, App),
            new ValidationError("Cannot remove a role when a contributor is assigned."));
    }

    [Fact]
    public void CanDelete_should_throw_exception_if_client_found()
    {
        App = App with
        {
            Roles = Roles.Empty.Add("clientRole")
        };

        var command = new DeleteRole { Name = "clientRole" };

        ValidationAssert.Throws(() => GuardAppRoles.CanDelete(command, App),
            new ValidationError("Cannot remove a role when a client is assigned."));
    }

    [Fact]
    public void CanDelete_should_throw_exception_if_default_role()
    {
        App = App with
        {
            Roles = Roles.Empty.Add(Role.Developer)
        };

        var command = new DeleteRole { Name = Role.Developer };

        ValidationAssert.Throws(() => GuardAppRoles.CanDelete(command, App),
            new ValidationError("Cannot delete a default role."));
    }

    [Fact]
    public void CanDelete_should_not_throw_exception_if_command_is_valid()
    {
        App = App with
        {
            Roles = Roles.Empty.Add(roleName)
        };

        var command = new DeleteRole { Name = roleName };

        GuardAppRoles.CanDelete(command, App);
    }

    [Fact]
    public void CanUpdate_should_throw_exception_if_name_empty()
    {
        App = App with
        {
            Roles = Roles.Empty.Add(roleName)
        };

        var command = new UpdateRole { Name = null!, Permissions = ["P1"] };

        ValidationAssert.Throws(() => GuardAppRoles.CanUpdate(command, App),
            new ValidationError("Name is required.", "Name"));
    }

    [Fact]
    public void CanUpdate_should_throw_exception_if_permission_is_null()
    {
        App = App with
        {
            Roles = Roles.Empty.Add(roleName)
        };

        var command = new UpdateRole { Name = roleName };

        ValidationAssert.Throws(() => GuardAppRoles.CanUpdate(command, App),
            new ValidationError("Permissions is required.", "Permissions"));
    }

    [Fact]
    public void CanUpdate_should_throw_exception_if_default_role()
    {
        App = App with
        {
            Roles = Roles.Empty.Add(Role.Developer)
        };

        var command = new UpdateRole { Name = Role.Developer, Permissions = ["P1"] };

        ValidationAssert.Throws(() => GuardAppRoles.CanUpdate(command, App),
            new ValidationError("Cannot update a default role."));
    }

    [Fact]
    public void CanUpdate_should_throw_exception_if_role_does_not_exists()
    {
        var command = new UpdateRole { Name = roleName, Permissions = ["P1"] };

        Assert.Throws<DomainObjectNotFoundException>(() => GuardAppRoles.CanUpdate(command, App));
    }

    [Fact]
    public void CanUpdate_should_not_throw_exception_if_properties_is_null()
    {
        App = App with
        {
            Roles = Roles.Empty.Add(roleName)
        };

        var command = new UpdateRole { Name = roleName, Permissions = ["P1"] };

        GuardAppRoles.CanUpdate(command, App);
    }

    [Fact]
    public void CanUpdate_should_not_throw_exception_if_role_exist_with_valid_command()
    {
        App = App with
        {
            Roles = Roles.Empty.Add(roleName)
        };

        var command = new UpdateRole { Name = roleName, Permissions = ["P1"] };

        GuardAppRoles.CanUpdate(command, App);
    }
}
