// ==========================================================================
//  AppClientsConverter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core.Apps.Json
{
    public sealed class AppClientsConverter : JsonClassConverter<AppClients>
    {
        protected override void WriteValue(JsonWriter writer, AppClients value, JsonSerializer serializer)
        {
            var json = new Dictionary<string, JsonAppClient>(value.Count);

            foreach (var client in value)
            {
                json.Add(client.Key, new JsonAppClient(client.Value));
            }

            serializer.Serialize(writer, json);
        }

        protected override AppClients ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            var json = serializer.Deserialize<Dictionary<string, JsonAppClient>>(reader);

            var clients = new AppClients();

            foreach (var client in json)
            {
                clients.Add(client.Key, client.Value.ToClient());
            }

            return clients;
        }
    }
}
