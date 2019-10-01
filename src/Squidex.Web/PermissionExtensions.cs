// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Squidex.Infrastructure.Security;
using AllPermissions = Squidex.Shared.Permissions;

namespace Squidex.Web
{
    public static class PermissionExtensions
    {
        public static PermissionSet Permissions(this HttpContext httpContext)
        {
            return httpContext.Context().Permissions;
        }

        public static bool Includes(this HttpContext httpContext, Permission permission, PermissionSet? additional = null)
        {
            return httpContext.Permissions().Includes(permission) || additional?.Includes(permission) == true;
        }

        public static bool Includes(this ApiController controller, Permission permission, PermissionSet? additional = null)
        {
            return controller.HttpContext.Includes(permission) || additional?.Includes(permission) == true;
        }

        public static bool HasPermission(this HttpContext httpContext, Permission permission, PermissionSet? additional = null)
        {
            return httpContext.Permissions().Allows(permission) || additional?.Allows(permission) == true;
        }

        public static bool HasPermission(this ApiController controller, Permission permission, PermissionSet? additional = null)
        {
            return controller.HttpContext.HasPermission(permission) || additional?.Allows(permission) == true;
        }

        public static bool HasPermission(this ApiController controller, string id, string app = "*", string schema = "*", PermissionSet? additional = null)
        {
            if (app == "*")
            {
                if (controller.RouteData.Values.TryGetValue("app", out var value) && value is string s)
                {
                    app = s;
                }
            }

            if (schema == "*")
            {
                if (controller.RouteData.Values.TryGetValue("name", out var value) && value is string s)
                {
                    schema = s;
                }
            }

            var permission = AllPermissions.ForApp(id, app, schema);

            return controller.HasPermission(permission, additional);
        }
    }
}
