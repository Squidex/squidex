// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Squidex.Infrastructure.Caching;

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public sealed class SeenEventsJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (existingValue is LRUCache<Guid, Guid> cache)
            {
                while (reader.Read())
                {
                    switch (reader.TokenType)
                    {
                        case JsonToken.Comment:
                            continue;
                        case JsonToken.EndArray:
                            break;
                        default:
                            var value = reader.Value;

                            if (value is string s && Guid.TryParse(s, out var guid))
                            {
                                cache.Set(guid, guid);
                            }

                            break;
                    }
                }
            }

            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is LRUCache<Guid, Guid> cache)
            {
                writer.WriteStartArray();

                foreach (var key in cache.Keys)
                {
                    writer.WriteValue(key);
                }

                writer.WriteEndArray();
            }
        }
    }
}
