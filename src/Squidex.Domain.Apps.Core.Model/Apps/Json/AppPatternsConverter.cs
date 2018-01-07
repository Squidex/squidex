// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Newtonsoft.Json;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core.Apps.Json
{
    public sealed class AppPatternsConverter : JsonClassConverter<AppPatterns>
    {
        protected override void WriteValue(JsonWriter writer, AppPatterns value, JsonSerializer serializer)
        {
            var json = new Dictionary<Guid, JsonAppPattern>(value.Count);

            foreach (var client in value)
            {
                json.Add(client.Key, new JsonAppPattern(client.Value));
            }

            serializer.Serialize(writer, json);
        }

        protected override AppPatterns ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            var json = serializer.Deserialize<Dictionary<Guid, JsonAppPattern>>(reader);

            return new AppPatterns(json.ToImmutableDictionary(x => x.Key, x => x.Value.ToPattern()));
        }
    }
}
