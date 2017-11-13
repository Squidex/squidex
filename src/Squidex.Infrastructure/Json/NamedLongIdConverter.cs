// ==========================================================================
//  NamedLongIdConverter.cs
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
    public sealed class NamedLongIdConverter : JsonClassConverter<NamedId<long>>
    {
        protected override void WriteValue(JsonWriter writer, NamedId<long> value, JsonSerializer serializer)
        {
            writer.WriteValue($"{value.Id},{value.Name}");
        }

        protected override NamedId<long> ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
            {
                throw new JsonException($"Expected String, but got {reader.TokenType}.");
            }

            var parts = reader.Value.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
            {
                throw new JsonException("Named id must have more than 2 parts divided by commata.");
            }

            if (!long.TryParse(parts[0], out var id))
            {
                throw new JsonException("Named id must be a valid long.");
            }

            return new NamedId<long>(id, string.Join(",", parts.Skip(1)));
        }
    }
}
