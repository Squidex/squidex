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

namespace Squidex.Domain.Apps.Core.Apps.Json
{
    public sealed class AppClientsConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var clients = (AppClients)value;

            var json = new Dictionary<string, JsonAppClient>(clients.Count);

            foreach (var client in clients)
            {
                json.Add(client.Key, new JsonAppClient(client.Value));
            }

            serializer.Serialize(writer, json);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var json = serializer.Deserialize<Dictionary<string, JsonAppClient>>(reader);

            var clients = new AppClients();

            foreach (var client in json)
            {
                clients.Add(client.Key, client.Value.ToClient());
            }

            return clients;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(AppClients);
        }
    }
}
