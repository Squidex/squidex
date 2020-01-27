﻿// ==========================================================================
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

namespace Squidex.Domain.Apps.Core.Apps.Json
{
    public sealed class AppClientsConverter : JsonClassConverter<AppClients>
    {
        protected override void WriteValue(JsonWriter writer, AppClients value, JsonSerializer serializer)
        {
            var json = new Dictionary<string, JsonAppClient>(value.Count);

            foreach (var (key, appClient) in value)
            {
                json.Add(key, new JsonAppClient(appClient));
            }

            serializer.Serialize(writer, json);
        }

        protected override AppClients ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            var json = serializer.Deserialize<Dictionary<string, JsonAppClient>>(reader)!;

            return new AppClients(json.ToDictionary(x => x.Key, x => x.Value.ToClient()));
        }
    }
}
