// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Security;

namespace Squidex.Domain.Apps.Core.Apps.Json
{
    public sealed class RolesSurrogate : Dictionary<string, JsonValue2>, ISurrogate<Roles>
    {
        public void FromSource(Roles source)
        {
            foreach (var customRole in source.Custom)
            {
                var permissions = new JsonArray();

                foreach (var permission in customRole.Permissions)
                {
                    permissions.Add(permission.Id);
                }

                var role =
                    new JsonObject()
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

                var properties = JsonValue2.Object();
                var permissions = PermissionSet.Empty;

                if (value.Type == JsonValueType.Array)
                {
                    var array = value.AsArray;

                    if (array.Count > 0)
                    {
                        permissions = new PermissionSet(array.Where(x => x.Type == JsonValueType.String).Select(x => x.AsString));
                    }
                }
                else if (value.Type == JsonValueType.Object)
                {
                    if (value.TryGetValue(JsonValueType.Array, "permissions", out var array))
                    {
                        permissions = new PermissionSet(array.AsArray.Where(x => x.Type == JsonValueType.String).Select(x => x.AsString));
                    }

                    if (!value.TryGetValue(JsonValueType.Object, "properties", out properties))
                    {
                        properties = JsonValue2.Object();
                    }
                }

                return new Role(key, permissions, properties);
            }));
        }
    }
}
