// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Security;

namespace Squidex.Domain.Apps.Core.Apps.Json;

public sealed class RolesSurrogate : Dictionary<string, JsonValue>, ISurrogate<Roles>
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

            var properties = new JsonObject();
            var permissions = PermissionSet.Empty;

            if (value.Value is JsonArray a)
            {
                if (a.Count > 0)
                {
                    permissions = new PermissionSet(a.Select(x => x.Value).OfType<string>());
                }
            }
            else if (value.Value is JsonObject o)
            {
                if (o.TryGetValue("permissions", out var found) && found.Value is JsonArray permissionArray)
                {
                    permissions = new PermissionSet(permissionArray.Select(x => x.Value).OfType<string>());
                }

                if (o.TryGetValue("properties", out found) && found.Value is JsonObject propertiesObject)
                {
                    properties = propertiesObject;
                }
            }

            return new Role(key, permissions, properties);
        }));
    }
}
