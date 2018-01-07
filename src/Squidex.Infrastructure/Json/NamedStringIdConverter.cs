// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using Newtonsoft.Json;

namespace Squidex.Infrastructure.Json
{
    public sealed class NamedStringIdConverter : JsonClassConverter<NamedId<string>>
    {
        protected override void WriteValue(JsonWriter writer, NamedId<string> value, JsonSerializer serializer)
        {
            writer.WriteValue($"{value.Id},{value.Name}");
        }

        protected override NamedId<string> ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
            {
                throw new JsonException($"Expected String, but got {reader.TokenType}.");
            }

            var parts = reader.Value.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
            {
                throw new JsonException("Named id must have more than 2 parts divided by colon.");
            }

            return new NamedId<string>(parts[0], string.Join(",", parts.Skip(1)));
        }
    }
}
