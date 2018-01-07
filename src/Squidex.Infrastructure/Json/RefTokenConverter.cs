// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;

namespace Squidex.Infrastructure.Json
{
    public sealed class RefTokenConverter : JsonClassConverter<RefToken>
    {
        protected override void WriteValue(JsonWriter writer, RefToken value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        protected override RefToken ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
            {
                throw new JsonException($"Expected String, but got {reader.TokenType}.");
            }

            return RefToken.Parse(reader.Value.ToString());
        }
    }
}