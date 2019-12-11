// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Newtonsoft;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Contents.Json
{
    public sealed class ContentFieldDataConverter : JsonClassConverter<ContentFieldData>
    {
        protected override void WriteValue(JsonWriter writer, ContentFieldData value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            foreach (var (key, jsonValue) in value)
            {
                writer.WritePropertyName(key);

                serializer.Serialize(writer, jsonValue);
            }

            writer.WriteEndObject();
        }

        protected override ContentFieldData ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            var result = new ContentFieldData();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        var propertyName = reader.Value!.ToString()!;

                        if (!reader.Read())
                        {
                            throw new JsonSerializationException("Unexpected end when reading Object.");
                        }

                        var value = serializer.Deserialize<IJsonValue>(reader)!;

                        if (Language.IsValidLanguage(propertyName) || propertyName == InvariantPartitioning.Key)
                        {
                            propertyName = string.Intern(propertyName);
                        }

                        result[propertyName] = value;
                        break;
                    case JsonToken.EndObject:
                        return result;
                }
            }

            throw new JsonSerializationException("Unexpected end when reading Object.");
        }
    }
}
