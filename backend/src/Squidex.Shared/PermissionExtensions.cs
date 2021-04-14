// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using Squidex.Infrastructure.Security;

namespace Squidex.Shared
{
    public static class PermissionExtensions
    {
        public static bool Allows(this PermissionSet permissions, string id, string app = Permission.Any, string schema = Permission.Any)
        {
            var permission = Permissions.ForApp(id, app, schema);

            return permissions.Allows(permission);
        }

        public static string[] ToAppNames(this PermissionSet permissions)
        {
            var matching = permissions.Where(x => x.StartsWith("squidex.apps."));

            var result =
                matching
                    .Select(x => x.Id.Split('.')).Where(x => x.Length > 2)
                    .Select(x => x[2])
                    .Distinct()
                    .ToArray();

            return result;
        }
    }
}
