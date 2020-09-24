// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Squidex.Infrastructure.Json.Newtonsoft;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Apps.Json
{
    public sealed class RoleConverter : JsonClassConverter<JsonRole>
    {
        protected override void WriteValue(JsonWriter writer, JsonRole value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("permissions");
            serializer.Serialize(writer, value.Permissions);

            writer.WritePropertyName("properties");
            serializer.Serialize(writer, value.Properties);

            writer.WriteEndObject();
        }

        protected override JsonRole ReadValue(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            var permissions = Array.Empty<string>();
            var properties = (JsonObject?)null;

            if (reader.TokenType == JsonToken.StartArray)
            {
                permissions = serializer.Deserialize<string[]>(reader)!;
            }
            else
            {
                while (reader.Read() && reader.TokenType != JsonToken.EndObject)
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        var propertyName = reader.Value!.ToString()!;

                        if (!reader.Read())
                        {
                            throw new JsonSerializationException("Unexpected end when reading role.");
                        }

                        switch (propertyName.ToLowerInvariant())
                        {
                            case "permissions":
                                permissions = serializer.Deserialize<string[]>(reader)!;
                                break;
                            case "properties":
                                properties = serializer.Deserialize<JsonObject>(reader)!;
                                break;
                        }
                    }
                }
            }

            return new JsonRole
            {
                Permissions = permissions,
                Properties = properties ?? JsonValue.Object()
            };
        }
    }
}
