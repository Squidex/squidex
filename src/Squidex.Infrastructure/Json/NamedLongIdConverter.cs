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

            try
            {
                return NamedId<long>.Parse(reader.Value.ToString(), long.TryParse);
            }
            catch (ArgumentException ex)
            {
                throw new JsonException(ex.Message);
            }
        }
    }
}
