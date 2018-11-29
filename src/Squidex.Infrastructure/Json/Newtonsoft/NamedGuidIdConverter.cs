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
    public sealed class NamedGuidIdConverter : JsonClassConverter<NamedId<Guid>>
    {
        protected override void WriteValue(JsonWriter writer, NamedId<Guid> value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        protected override NamedId<Guid> ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
            {
                throw new JsonException($"Expected String, but got {reader.TokenType}.");
            }

            if (!NamedId<Guid>.TryParse(reader.Value.ToString(), Guid.TryParse, out var result))
            {
                throw new JsonException("Named id must have more than 2 parts divided by commata.");
            }

            return result;
        }
    }
}
