// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Shared.Identity;

namespace Squidex.Web
{
    public static class PermissionExtensions
    {
        private sealed class PermissionFeature
        {
            public PermissionSet Permissions { get; }

            public PermissionFeature(PermissionSet permissions)
            {
                Permissions = permissions;
            }
        }

        public static PermissionSet GetPermissions(this HttpContext httpContext)
        {
            var feature = httpContext.Features.Get<PermissionFeature>();

            if (feature == null)
            {
                feature = new PermissionFeature(httpContext.User.Permissions());

                httpContext.Features.Set(feature);
            }

            return feature.Permissions;
        }

        public static bool HasPermission(this HttpContext httpContext, Permission permission)
        {
            return httpContext.GetPermissions().Includes(permission);
        }

        public static bool HasPermission(this HttpContext httpContext, string id, string app = "*", string schema = "*")
        {
            return httpContext.GetPermissions().Includes(Permissions.ForApp(id, app, schema));
        }

        public static bool HasPermission(this ApiController controller, Permission permission)
        {
            return controller.HttpContext.GetPermissions().Includes(permission);
        }

        public static bool HasPermission(this ApiController controller, string id, string app = "*", string schema = "*")
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

            return controller.HttpContext.GetPermissions().Includes(Permissions.ForApp(id, app, schema));
        }
    }
}
