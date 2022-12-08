// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Contents.Json;

public sealed class ContentFieldDataConverter : JsonConverter<ContentFieldData>
{
    public override ContentFieldData? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var result = new ContentFieldData();

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.PropertyName:
                    var propertyName = reader.GetString()!;

                    if (!reader.Read())
                    {
                        throw new JsonException("Unexpected end when reading Object.");
                    }

                    var value = JsonSerializer.Deserialize<JsonValue>(ref reader, options)!;

                    if (propertyName == InvariantPartitioning.Key)
                    {
                        propertyName = InvariantPartitioning.Key;
                    }
                    else if (Language.TryGetLanguage(propertyName, out var language))
                    {
                        propertyName = language.Iso2Code;
                    }

                    result[propertyName] = value;
                    break;
                case JsonTokenType.EndObject:
                    return result;
            }
        }

        throw new JsonException("Unexpected end when reading Object.");
    }

    public override void Write(Utf8JsonWriter writer, ContentFieldData value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (var (key, jsonValue) in value)
        {
            writer.WritePropertyName(key);

            JsonSerializer.Serialize(writer, jsonValue, options);
        }

        writer.WriteEndObject();
    }
}
