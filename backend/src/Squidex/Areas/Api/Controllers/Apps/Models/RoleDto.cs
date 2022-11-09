// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Apps.Models;

public sealed class RoleDto : Resource
{
    /// <summary>
    /// The role name.
    /// </summary>
    [LocalizedRequired]
    public string Name { get; set; }

    /// <summary>
    /// The number of clients with this role.
    /// </summary>
    public int NumClients { get; set; }

    /// <summary>
    /// The number of contributors with this role.
    /// </summary>
    public int NumContributors { get; set; }

    /// <summary>
    /// Indicates if the role is an builtin default role.
    /// </summary>
    public bool IsDefaultRole { get; set; }

    /// <summary>
    /// Associated list of permissions.
    /// </summary>
    [LocalizedRequired]
    public string[] Permissions { get; set; }

    /// <summary>
    /// Associated list of UI properties.
    /// </summary>
    [LocalizedRequired]
    public JsonObject Properties { get; set; }

    public static RoleDto FromDomain(Role role, IAppEntity app)
    {
        var result = new RoleDto
        {
            Name = role.Name,
            NumClients = GetNumClients(role, app),
            NumContributors = GetNumContributors(role, app),
            Permissions = role.Permissions.ToIds().ToArray(),
            Properties = role.Properties,
            IsDefaultRole = role.IsDefault
        };

        return result;
    }

    private static int GetNumContributors(Role role, IAppEntity app)
    {
        return app.Contributors.Count(x => role.Equals(x.Value));
    }

    private static int GetNumClients(Role role, IAppEntity app)
    {
        return app.Clients.Count(x => role.Equals(x.Value.Role));
    }

    public RoleDto CreateLinks(Resources resources)
    {
        var values = new { app = resources.App, roleName = Name };

        if (!IsDefaultRole)
        {
            if (resources.CanUpdateRole)
            {
                AddPutLink("update",
                    resources.Url<AppRolesController>(x => nameof(x.PutRole), values));
            }

            if (resources.CanDeleteRole && NumClients == 0 && NumContributors == 0)
            {
                AddDeleteLink("delete",
                    resources.Url<AppRolesController>(x => nameof(x.DeleteRole), values));
            }
        }

        return this;
    }
}
