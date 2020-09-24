// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Squidex.Infrastructure.Json.Newtonsoft;
using Squidex.Infrastructure.Security;

namespace Squidex.Domain.Apps.Core.Apps.Json
{
    public sealed class RolesConverter : JsonClassConverter<Roles>
    {
        protected override void WriteValue(JsonWriter writer, Roles value, JsonSerializer serializer)
        {
            var json = new Dictionary<string, JsonRole>(value.CustomCount);

            foreach (var role in value.Custom)
            {
                json.Add(role.Name, new JsonRole
                {
                    Permissions = role.Permissions.ToIds().ToArray(),
                    Properties = role.Properties
                });
            }

            serializer.Serialize(writer, json);
        }

        protected override Roles ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            var json = serializer.Deserialize<Dictionary<string, JsonRole>>(reader)!;

            if (json.Count == 0)
            {
                return Roles.Empty;
            }

            return new Roles(json.ToDictionary(x => x.Key, x =>
            {
                var permissions = PermissionSet.Empty;

                if (x.Value.Permissions.Length > 0)
                {
                    permissions = new PermissionSet(x.Value.Permissions);
                }

                return new Role(x.Key, permissions, x.Value.Properties);
            }));
        }
    }
}
