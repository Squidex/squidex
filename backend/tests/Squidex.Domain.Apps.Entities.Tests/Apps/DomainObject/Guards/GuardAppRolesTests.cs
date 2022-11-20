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

#pragma warning disable SA1310 // Field names must not contain underscore

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject.Guards;

public class GuardAppRolesTests : IClassFixture<TranslationsFixture>
{
    private readonly string roleName = "Role1";
    private readonly Roles roles_0 = Roles.Empty;
    private readonly AppClients clients = AppClients.Empty.Add("client", "secret", "clientRole");
    private readonly Contributors contributors = Contributors.Empty.Assign("contributor", "contributorRole");

    [Fact]
    public void CanAdd_should_throw_exception_if_name_empty()
    {
        var command = new AddRole { Name = null! };

        ValidationAssert.Throws(() => GuardAppRoles.CanAdd(command, App(roles_0)),
            new ValidationError("Name is required.", "Name"));
    }

    [Fact]
    public void CanAdd_should_throw_exception_if_name_exists()
    {
        var roles_1 = roles_0.Add(roleName);

        var command = new AddRole { Name = roleName };

        ValidationAssert.Throws(() => GuardAppRoles.CanAdd(command, App(roles_1)),
            new ValidationError("A role with the same name already exists."));
    }

    [Fact]
    public void CanAdd_should_not_throw_exception_if_command_is_valid()
    {
        var command = new AddRole { Name = roleName };

        GuardAppRoles.CanAdd(command, App(roles_0));
    }

    [Fact]
    public void CanDelete_should_throw_exception_if_name_empty()
    {
        var command = new DeleteRole { Name = null! };

        ValidationAssert.Throws(() => GuardAppRoles.CanDelete(command, App(roles_0)),
            new ValidationError("Name is required.", "Name"));
    }

    [Fact]
    public void CanDelete_should_throw_exception_if_role_not_found()
    {
        var command = new DeleteRole { Name = roleName };

        Assert.Throws<DomainObjectNotFoundException>(() => GuardAppRoles.CanDelete(command, App(roles_0)));
    }

    [Fact]
    public void CanDelete_should_throw_exception_if_contributor_found()
    {
        var roles_1 = roles_0.Add("contributorRole");

        var command = new DeleteRole { Name = "contributorRole" };

        ValidationAssert.Throws(() => GuardAppRoles.CanDelete(command, App(roles_1)),
            new ValidationError("Cannot remove a role when a contributor is assigned."));
    }

    [Fact]
    public void CanDelete_should_throw_exception_if_client_found()
    {
        var roles_1 = roles_0.Add("clientRole");

        var command = new DeleteRole { Name = "clientRole" };

        ValidationAssert.Throws(() => GuardAppRoles.CanDelete(command, App(roles_1)),
            new ValidationError("Cannot remove a role when a client is assigned."));
    }

    [Fact]
    public void CanDelete_should_throw_exception_if_default_role()
    {
        var roles_1 = roles_0.Add(Role.Developer);

        var command = new DeleteRole { Name = Role.Developer };

        ValidationAssert.Throws(() => GuardAppRoles.CanDelete(command, App(roles_1)),
            new ValidationError("Cannot delete a default role."));
    }

    [Fact]
    public void CanDelete_should_not_throw_exception_if_command_is_valid()
    {
        var roles_1 = roles_0.Add(roleName);

        var command = new DeleteRole { Name = roleName };

        GuardAppRoles.CanDelete(command, App(roles_1));
    }

    [Fact]
    public void CanUpdate_should_throw_exception_if_name_empty()
    {
        var roles_1 = roles_0.Add(roleName);

        var command = new UpdateRole { Name = null!, Permissions = new[] { "P1" } };

        ValidationAssert.Throws(() => GuardAppRoles.CanUpdate(command, App(roles_1)),
            new ValidationError("Name is required.", "Name"));
    }

    [Fact]
    public void CanUpdate_should_throw_exception_if_permission_is_null()
    {
        var roles_1 = roles_0.Add(roleName);

        var command = new UpdateRole { Name = roleName };

        ValidationAssert.Throws(() => GuardAppRoles.CanUpdate(command, App(roles_1)),
            new ValidationError("Permissions is required.", "Permissions"));
    }

    [Fact]
    public void CanUpdate_should_throw_exception_if_default_role()
    {
        var roles_1 = roles_0.Add(Role.Developer);

        var command = new UpdateRole { Name = Role.Developer, Permissions = new[] { "P1" } };

        ValidationAssert.Throws(() => GuardAppRoles.CanUpdate(command, App(roles_1)),
            new ValidationError("Cannot update a default role."));
    }

    [Fact]
    public void CanUpdate_should_throw_exception_if_role_does_not_exists()
    {
        var command = new UpdateRole { Name = roleName, Permissions = new[] { "P1" } };

        Assert.Throws<DomainObjectNotFoundException>(() => GuardAppRoles.CanUpdate(command, App(roles_0)));
    }

    [Fact]
    public void CanUpdate_should_not_throw_exception_if_properties_is_null()
    {
        var roles_1 = roles_0.Add(roleName);

        var command = new UpdateRole { Name = roleName, Permissions = new[] { "P1" } };

        GuardAppRoles.CanUpdate(command, App(roles_1));
    }

    [Fact]
    public void CanUpdate_should_not_throw_exception_if_role_exist_with_valid_command()
    {
        var roles_1 = roles_0.Add(roleName);

        var command = new UpdateRole { Name = roleName, Permissions = new[] { "P1" } };

        GuardAppRoles.CanUpdate(command, App(roles_1));
    }

    private IAppEntity App(Roles roles)
    {
        var app = A.Fake<IAppEntity>();

        A.CallTo(() => app.Contributors).Returns(contributors);
        A.CallTo(() => app.Clients).Returns(clients);
        A.CallTo(() => app.Roles).Returns(roles);

        return app;
    }
}
