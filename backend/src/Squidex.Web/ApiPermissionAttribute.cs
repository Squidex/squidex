// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Infrastructure.Security;
using Squidex.Shared;

namespace Squidex.Web;

public class ApiPermissionAttribute : AuthorizeAttribute, IAsyncActionFilter
{
    private readonly string[] permissionIds;

    public IEnumerable<string> PermissionIds
    {
        get => permissionIds;
    }

    public ApiPermissionAttribute(params string[] ids)
    {
        AuthenticationSchemes = Constants.ApiSecurityScheme;

        permissionIds = ids;
    }

    public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (permissionIds.Length > 0)
        {
            var permissions = context.HttpContext.Context().UserPermissions;

            var hasPermission = false;

            if (permissions != null)
            {
                foreach (var id in permissionIds)
                {
                    var app = context.HttpContext.Features.Get<IAppFeature>()?.App.Name;

                    if (string.IsNullOrWhiteSpace(app))
                    {
                        app = Permission.Any;
                    }

                    var schema = context.HttpContext.Features.Get<ISchemaFeature>()?.Schema.SchemaDef.Name;

                    if (string.IsNullOrWhiteSpace(schema))
                    {
                        schema = Permission.Any;
                    }

                    var team = context.HttpContext.Features.Get<ITeamFeature>()?.Team.Id.ToString();

                    if (string.IsNullOrWhiteSpace(team))
                    {
                        team = Permission.Any;
                    }

                    if (permissions.Allows(id, app, schema, team))
                    {
                        hasPermission = true;
                        break;
                    }
                }
            }

            if (!hasPermission)
            {
                context.Result = new StatusCodeResult(403);

                return Task.CompletedTask;
            }
        }

        return next();
    }
}
