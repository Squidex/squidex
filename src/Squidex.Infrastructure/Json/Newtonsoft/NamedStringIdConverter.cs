// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;

namespace Squidex.Infrastructure.Json.Newtonsoft
{
    public sealed class NamedStringIdConverter : JsonClassConverter<NamedId<string>>
    {
        protected override void WriteValue(JsonWriter writer, NamedId<string> value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        protected override NamedId<string> ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
            {
                throw new JsonException($"Expected String, but got {reader.TokenType}.");
            }

            if (!NamedId<string>.TryParse(reader.Value.ToString(), ParseString, out var result))
            {
                throw new JsonException("Named id must have at least 2 parts divided by commata.");
            }

            return result;
        }

        private static bool ParseString(string value, out string result)
        {
            result = value;

            return true;
        }
    }
}
