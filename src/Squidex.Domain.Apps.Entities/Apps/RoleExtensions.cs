// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using Squidex.Infrastructure.Security;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public static class RoleExtensions
    {
        public static string[] Prefix(this string[] permissions, string name)
        {
            var result = new string[permissions.Length + 1];

            result[0] = Permissions.ForApp(Permissions.AppCommon, name).Id;

            if (permissions.Length > 0)
            {
                var prefix = Permissions.ForApp(Permissions.App, name).Id;

                for (var i = 0; i < permissions.Length; i++)
                {
                    result[i + 1] = string.Concat(prefix, ".", permissions[i]);
                }
            }

            permissions = result.Distinct().ToArray();

            return permissions;
        }

        public static PermissionSet WithoutApp(this PermissionSet set, string name)
        {
            var prefix = Permissions.ForApp(Permissions.App, name).Id;

            return new PermissionSet(set.Select(x =>
            {
                var id = x.Id;

                if (string.Equals(id, prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return Permission.Any;
                }
                else if (id.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return id.Substring(prefix.Length + 1);
                }
                else
                {
                    return id;
                }
            }).Where(x => x != "common"));
        }
    }
}
