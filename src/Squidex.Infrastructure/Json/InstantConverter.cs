// ==========================================================================
//  InstantConverter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Text;

namespace Squidex.Infrastructure.Json
{
    public sealed class InstantConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null)
            {
                writer.WriteValue(value.ToString());
            }
            else
            {
                writer.WriteNull();
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                return InstantPattern.General.Parse(reader.Value.ToString()).Value;
            }

            if (reader.TokenType == JsonToken.Date)
            {
                return Instant.FromDateTimeUtc((DateTime)reader.Value);
            }

            if (reader.TokenType == JsonToken.Null && objectType == typeof(Instant?))
            {
                return null;
            }

            throw new JsonException($"Not a valid date time, expected String or Date, but got {reader.TokenType}.");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Instant) || objectType == typeof(Instant?);
        }
    }
}
