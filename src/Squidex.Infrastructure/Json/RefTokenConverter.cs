// ==========================================================================
//  RefTokenConverter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json;

namespace Squidex.Infrastructure.Json
{
    public sealed class RefTokenConverter : JsonClassConverter<RefToken>
    {
        protected override void WriteValue(JsonWriter writer, RefToken value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        protected override RefToken ReadValue(JsonReader reader, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
            {
                throw new JsonException($"Expected String, but got {reader.TokenType}.");
            }

            return RefToken.Parse(reader.Value.ToString());
        }
    }
}