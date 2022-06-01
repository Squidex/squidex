// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json;
using Squidex.Infrastructure.Json.Objects;

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator

namespace Squidex.Infrastructure.Json.Newtonsoft
{
    public class JsonValueConverter : JsonConverter, ISupportedTypes
    {
        private readonly HashSet<Type> supportedTypes = new HashSet<Type>
        {
            typeof(JsonValue)
        };

        public virtual IEnumerable<Type> SupportedTypes
        {
            get => supportedTypes;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            return ReadJson(reader);
        }

        private static JsonValue ReadJson(JsonReader reader)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Comment:
                    reader.Read();
                    break;
                case JsonToken.StartObject:
                    {
                        var result = new JsonObject(1);

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

                                    var value = ReadJson(reader);

                                    result[propertyName] = value;
                                    break;
                                case JsonToken.EndObject:
                                    return result;
                            }
                        }

                        throw new JsonSerializationException("Unexpected end when reading Object.");
                    }

                case JsonToken.StartArray:
                    {
                        var result = new JsonArray(1);

                        while (reader.Read())
                        {
                            switch (reader.TokenType)
                            {
                                case JsonToken.Comment:
                                    continue;
                                case JsonToken.EndArray:
                                    return result;
                                default:
                                    var value = ReadJson(reader);

                                    result.Add(value);
                                    break;
                            }
                        }

                        throw new JsonSerializationException("Unexpected end when reading Object.");
                    }

                case JsonToken.Integer when reader.Value is int i:
                    return (double)i;
                case JsonToken.Integer when reader.Value is long l:
                    return (double)l;
                case JsonToken.Float when reader.Value is float f:
                    return (double)f;
                case JsonToken.Float when reader.Value is double d:
                    return d;
                case JsonToken.Boolean when reader.Value is bool b:
                    return b;
                case JsonToken.Date when reader.Value is DateTime d:
                    return d.ToIso8601();
                case JsonToken.String when reader.Value is string s:
                    return s;
                case JsonToken.Null:
                case JsonToken.Undefined:
                    return default;
            }

            throw new NotSupportedException();
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            WriteJson(writer, (JsonValue)value);
        }

        private static void WriteJson(JsonWriter writer, JsonValue value)
        {
            switch (value.Type)
            {
                case JsonValueType.Null:
                    writer.WriteNull();
                    break;
                case JsonValueType.Boolean:
                    writer.WriteValue(value.AsBoolean);
                    break;
                case JsonValueType.String:
                    writer.WriteValue(value.AsString);
                    break;
                case JsonValueType.Number:
                    var number = value.AsNumber;

                    if (number % 1 == 0)
                    {
                        writer.WriteValue((long)number);
                    }
                    else
                    {
                        writer.WriteValue(number);
                    }

                    break;
                case JsonValueType.Array:
                    writer.WriteStartArray();

                    foreach (var item in value.AsArray)
                    {
                        WriteJson(writer, item);
                    }

                    writer.WriteEndArray();
                    break;

                case JsonValueType.Object:
                    writer.WriteStartObject();

                    foreach (var (key, jsonValue) in value.AsObject)
                    {
                        writer.WritePropertyName(key);

                        WriteJson(writer, jsonValue);
                    }

                    writer.WriteEndObject();
                    break;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return supportedTypes.Contains(objectType);
        }
    }
}
