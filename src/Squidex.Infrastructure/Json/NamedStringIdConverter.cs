// ==========================================================================
//  NamedStringIdConverter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using Newtonsoft.Json;

namespace Squidex.Infrastructure.Json
{
    public sealed class NamedStringIdConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var namedId = (NamedId<string>)value;

            writer.WriteValue($"{namedId.Id},{namedId.Name}");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var parts = ((string)reader.Value).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
            {
                throw new JsonException("Named id must have more than 2 parts divided by colon.");
            }

            return new NamedId<string>(parts[0], string.Join(",", parts.Skip(1)));
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(NamedId<string>);
        }
    }
}
