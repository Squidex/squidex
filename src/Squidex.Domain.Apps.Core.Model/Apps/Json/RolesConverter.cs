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
            var json = new Dictionary<string, string[]>(value.Count);

            foreach (var role in value)
            {
                json.Add(role.Key, role.Value.Permissions.ToIds().ToArray());
            }

            serializer.Serialize(writer, json);
        }

        protected override Roles ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            var json = serializer.Deserialize<Dictionary<string, string[]>>(reader);

            return new Roles(json.Select(Convert).ToArray());
        }

        private static KeyValuePair<string, Role> Convert(KeyValuePair<string, string[]> kvp)
        {
            return new KeyValuePair<string, Role>(kvp.Key, new Role(kvp.Key, new PermissionSet(kvp.Value)));
        }
    }
}
