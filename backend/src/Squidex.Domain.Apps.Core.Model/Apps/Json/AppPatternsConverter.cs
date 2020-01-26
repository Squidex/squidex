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

namespace Squidex.Domain.Apps.Core.Apps.Json
{
    public sealed class AppPatternsConverter : JsonClassConverter<AppPatterns>
    {
        protected override void WriteValue(JsonWriter writer, AppPatterns value, JsonSerializer serializer)
        {
            var json = new Dictionary<Guid, JsonAppPattern>(value.Count);

            foreach (var (key, appPattern) in value)
            {
                json.Add(key, new JsonAppPattern(appPattern));
            }

            serializer.Serialize(writer, json);
        }

        protected override AppPatterns ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            var json = serializer.Deserialize<Dictionary<Guid, JsonAppPattern>>(reader)!;

            return new AppPatterns(json.ToDictionary(x => x.Key, x => x.Value.ToPattern()));
        }
    }
}
