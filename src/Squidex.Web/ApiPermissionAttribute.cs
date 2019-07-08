// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Web
{
    public sealed class ApiPermissionAttribute : AuthorizeAttribute, IAsyncActionFilter
    {
        private readonly string[] permissionIds;

        public IEnumerable<string> PermissionIds
        {
            get { return permissionIds; }
        }

        public ApiPermissionAttribute(params string[] ids)
        {
            AuthenticationSchemes = "Bearer";

            permissionIds = ids;
        }

        public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (permissionIds.Length > 0)
            {
                var permissions = context.HttpContext.Context().Permissions;

                var hasPermission = false;

                if (permissions != null)
                {
                    foreach (var permissionId in permissionIds)
                    {
                        var id = permissionId;

                        foreach (var routeParam in context.RouteData.Values)
                        {
                            id = id.Replace($"{{{routeParam.Key}}}", routeParam.Value?.ToString());
                        }

                        if (permissions.Allows(new Permission(id)))
                        {
                            hasPermission = true;
                            break;
                        }
                    }
                }

                if (!hasPermission)
                {
                    context.Result = new StatusCodeResult(403);

                    return TaskHelper.Done;
                }
            }

            return next();
        }
    }
}
