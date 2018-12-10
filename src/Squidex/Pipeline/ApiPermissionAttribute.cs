// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Tasks;
using Squidex.Shared.Identity;

namespace Squidex.Pipeline
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
            AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme;

            permissionIds = ids;
        }

        public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (permissionIds.Length > 0)
            {
                var set = context.HttpContext.User.Permissions();

                var hasPermission = false;

                foreach (var permissionId in permissionIds)
                {
                    var id = permissionId;

                    foreach (var routeParam in context.RouteData.Values)
                    {
                        id = id.Replace($"{{{routeParam.Key}}}", routeParam.Value?.ToString());
                    }

                    hasPermission |= set.Allows(new Permission(id));
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
