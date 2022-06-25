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
            var previousDateParseHandling = reader.DateParseHandling;

            reader.DateParseHandling = DateParseHandling.None;
            try
            {
                return ReadJsonCore(reader);
            }
            finally
            {
                reader.DateParseHandling = previousDateParseHandling;
            }
        }

        private static JsonValue ReadJsonCore(JsonReader reader)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Null:
                    return default;
                case JsonToken.Undefined:
                    return default;
                case JsonToken.String:
                    return new JsonValue((string)reader.Value!);
                case JsonToken.Integer:
                    return new JsonValue((long)reader.Value!);
                case JsonToken.Float:
                    return new JsonValue((double)reader.Value!);
                case JsonToken.Boolean:
                    return (bool)reader.Value! ? JsonValue.True : JsonValue.False;
                case JsonToken.StartObject:
                    {
                        var result = new JsonObject(4);

                        Dictionary<string, JsonValue> dictionary = result;

                        while (reader.Read())
                        {
                            switch (reader.TokenType)
                            {
                                case JsonToken.PropertyName:
                                    var propertyName = reader.Value!.ToString()!;

                                    if (!reader.Read())
                                    {
                                        ThrowInvalidObjectException();
                                    }

                                    var value = ReadJsonCore(reader);

                                    dictionary.Add(propertyName, value);
                                    break;
                                case JsonToken.EndObject:
                                    result.TrimExcess();

                                    return new JsonValue(result);
                            }
                        }

                        ThrowInvalidObjectException();
                        return default!;
                    }

                case JsonToken.StartArray:
                    {
                        var result = new JsonArray(4);

                        while (reader.Read())
                        {
                            switch (reader.TokenType)
                            {
                                case JsonToken.Comment:
                                    continue;
                                case JsonToken.EndArray:
                                    result.TrimExcess();

                                    return new JsonValue(result);
                                default:
                                    var value = ReadJsonCore(reader);

                                    result.Add(value);
                                    break;
                            }
                        }

                        ThrowInvalidArrayException();
                        return default!;
                    }

                case JsonToken.Comment:
                    reader.Read();
                    break;
            }

            ThrowUnsupportedTypeException();
            return default;
        }

        private static void ThrowUnsupportedTypeException()
        {
            throw new JsonSerializationException("Unsupported type.");
        }

        private static void ThrowInvalidArrayException()
        {
            throw new JsonSerializationException("Unexpected end when reading Array.");
        }

        private static void ThrowInvalidObjectException()
        {
            throw new JsonSerializationException("Unexpected end when reading Object.");
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
            switch (value.Value)
            {
                case null:
                    writer.WriteNull();
                    break;
                case bool b:
                    writer.WriteValue(b);
                    break;
                case string s:
                    writer.WriteValue(s);
                    break;
                case double n:
                    if (n % 1 == 0)
                    {
                        writer.WriteValue((long)n);
                    }
                    else
                    {
                        writer.WriteValue(n);
                    }

                    break;
                case JsonArray a:
                    writer.WriteStartArray();

                    foreach (var item in a)
                    {
                        WriteJson(writer, item);
                    }

                    writer.WriteEndArray();
                    break;

                case JsonObject o:
                    writer.WriteStartObject();

                    foreach (var (key, jsonValue) in o)
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
