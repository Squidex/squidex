// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Security;

namespace Squidex.Domain.Apps.Core.Apps.Json
{
    public sealed class RolesSurrogate : Dictionary<string, IJsonValue>, ISurrogate<Roles>
    {
        public void FromSource(Roles source)
        {
            foreach (var customRole in source.Custom)
            {
                var permissions = JsonValue.Array();

                foreach (var permission in customRole.Permissions)
                {
                    permissions.Add(JsonValue.Create(permission.Id));
                }

                var role =
                    JsonValue.Object()
                        .Add("permissions", permissions)
                        .Add("properties", customRole.Properties);

                Add(customRole.Name, role);
            }
        }

        public Roles ToSource()
        {
            if (Count == 0)
            {
                return Roles.Empty;
            }

            return new Roles(this.ToDictionary(x => x.Key, x =>
            {
                var (key, value) = x;

                var properties = JsonValue.Object();
                var permissions = PermissionSet.Empty;

                if (value is JsonArray array)
                {
                    if (array.Count > 0)
                    {
                        permissions = new PermissionSet(array.OfType<JsonString>().Select(x => x.ToString()));
                    }
                }
                else if (value is JsonObject obj)
                {
                    if (obj.TryGetValue("permissions", out array!) && array.Count > 0)
                    {
                        permissions = new PermissionSet(array.OfType<JsonString>().Select(x => x.ToString()));
                    }

                    if (!obj.TryGetValue<JsonObject>("properties", out properties))
                    {
                        properties = JsonValue.Object();
                    }
                }

                return new Role(key, permissions, properties);
            }));
        }
    }
}
