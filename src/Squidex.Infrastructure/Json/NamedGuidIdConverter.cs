// ==========================================================================
//  NamedGuidIdConverter.cs
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
    public sealed class NamedGuidIdConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var namedId = (NamedId<Guid>)value;

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
                throw new JsonException("Named id must have more than 2 parts divided by commata");
            }

            if (!Guid.TryParse(parts[0], out var id))
            {
                throw new JsonException("Named id must be a valid guid");
            }

            return new NamedId<Guid>(id, string.Join(",", parts.Skip(1)));
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(NamedId<Guid>);
        }
    }
}
