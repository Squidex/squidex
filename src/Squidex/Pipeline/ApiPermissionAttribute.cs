// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Infrastructure.Security;

namespace Squidex.Pipeline
{
    public sealed class ApiPermissionAttribute : AuthorizeAttribute, IAsyncActionFilter
    {
        private readonly string permissionId;

        public ApiPermissionAttribute(string id = null)
        {
            AuthenticationSchemes = IdentityServerAuthenticationDefaults.AuthenticationScheme;

            permissionId = id;
        }

        public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (permissionId != null)
            {
                var id = permissionId;

                foreach (var routeParam in context.RouteData.Values)
                {
                    id = id.Replace($"{{{routeParam.Key}}}", routeParam.Value?.ToString());
                }

                var set = new PermissionSet(
                    context.HttpContext.User.FindAll("Permission")
                        .Select(x => x.Value)
                        .Select(x => new Permission(x)));

                if (!set.GivesPermissionTo(new Permission(id)))
                {
                    // context.Result = new StatusCodeResult(403);
                }
            }

            return next();
        }
    }
}
