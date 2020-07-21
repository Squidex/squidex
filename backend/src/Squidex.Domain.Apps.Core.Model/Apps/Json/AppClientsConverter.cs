// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Squidex.Infrastructure.Json.Newtonsoft;

namespace Squidex.Domain.Apps.Core.Apps.Json
{
    public sealed class AppClientsConverter : JsonClassConverter<AppClients>
    {
        protected override void WriteValue(JsonWriter writer, AppClients value, JsonSerializer serializer)
        {
            var json = new Dictionary<string, AppClient>(value.Count);

            foreach (var (key, client) in value)
            {
                json.Add(key, client);
            }

            serializer.Serialize(writer, json);
        }

        protected override AppClients ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            var json = serializer.Deserialize<Dictionary<string, AppClient>>(reader)!;

            return new AppClients(json);
        }
    }
}
