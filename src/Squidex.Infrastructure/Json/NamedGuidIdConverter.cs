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
    public sealed class NamedGuidIdConverter : JsonClassConverter<NamedId<Guid>>
    {
        protected override void WriteValue(JsonWriter writer, NamedId<Guid> value, JsonSerializer serializer)
        {
            writer.WriteValue($"{value.Id},{value.Name}");
        }

        protected override NamedId<Guid> ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String)
            {
                throw new JsonException($"Expected String, but got {reader.TokenType}.");
            }

            try
            {
                return NamedId<Guid>.Parse(reader.Value.ToString(), Guid.TryParse);
            }
            catch (ArgumentException ex)
            {
                throw new JsonException(ex.Message);
            }
        }
    }
}
