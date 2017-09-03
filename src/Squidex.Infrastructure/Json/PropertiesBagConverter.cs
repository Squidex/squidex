// ==========================================================================
//  PropertiesBagConverter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Extensions;

namespace Squidex.Infrastructure.Json
{
    public sealed class PropertiesBagConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(PropertiesBag).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var properties = new PropertiesBag();

            while (reader.Read())
            {
                if (reader.TokenType != JsonToken.PropertyName)
                {
                    break;
                }

                var key = reader.Value.ToString();

                reader.Read();

                var value = reader.Value;

                if (value is DateTime dateTime)
                {
                    properties.Set(key, dateTime.ToInstant());
                }
                else
                {
                    properties.Set(key, value);
                }
            }

            return properties;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var properties = (PropertiesBag)value;

            writer.WriteStartObject();

            foreach (var kvp in properties.Properties)
            {
                writer.WritePropertyName(kvp.Key);

                if (kvp.Value.RawValue is Instant)
                {
                    writer.WriteValue(kvp.Value.ToString());
                }
                else
                {
                    writer.WriteValue(kvp.Value.RawValue);
                }
            }

            writer.WriteEndObject();
        }
    }
}
