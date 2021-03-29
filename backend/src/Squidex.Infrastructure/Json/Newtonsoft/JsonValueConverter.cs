// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Squidex.Infrastructure.Json.Objects;

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator

namespace Squidex.Infrastructure.Json.Newtonsoft
{
    public class JsonValueConverter : JsonConverter, ISupportedTypes
    {
        private readonly HashSet<Type> supportedTypes = new HashSet<Type>
        {
            typeof(IJsonValue),
            typeof(JsonArray),
            typeof(JsonBoolean),
            typeof(JsonNull),
            typeof(JsonNumber),
            typeof(JsonObject),
            typeof(JsonString)
        };

        public virtual IEnumerable<Type> SupportedTypes
        {
            get => supportedTypes;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            return ReadJson(reader);
        }

        private static IJsonValue ReadJson(JsonReader reader)
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
                    return JsonValue.Create(i);
                case JsonToken.Integer when reader.Value is long l:
                    return JsonValue.Create(l);
                case JsonToken.Float when reader.Value is float f:
                    return JsonValue.Create(f);
                case JsonToken.Float when reader.Value is double d:
                    return JsonValue.Create(d);
                case JsonToken.Boolean when reader.Value is bool b:
                    return JsonValue.Create(b);
                case JsonToken.Date when reader.Value is DateTime d:
                    return JsonValue.Create(d.ToIso8601());
                case JsonToken.String when reader.Value is string s:
                    return JsonValue.Create(s);
                case JsonToken.Null:
                case JsonToken.Undefined:
                    return JsonValue.Null;
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

            WriteJson(writer, (IJsonValue)value);
        }

        private static void WriteJson(JsonWriter writer, IJsonValue value)
        {
            switch (value)
            {
                case JsonNull:
                    writer.WriteNull();
                    break;
                case JsonBoolean s:
                    writer.WriteValue(s.Value);
                    break;
                case JsonString s:
                    writer.WriteValue(s.Value);
                    break;
                case JsonNumber s:
                    if (s.Value % 1 == 0)
                    {
                        writer.WriteValue((long)s.Value);
                    }
                    else
                    {
                        writer.WriteValue(s.Value);
                    }

                    break;
                case JsonArray array:
                    writer.WriteStartArray();

                    for (var i = 0; i < array.Count; i++)
                    {
                        WriteJson(writer, array[i]);
                    }

                    writer.WriteEndArray();
                    break;

                case JsonObject obj:
                    writer.WriteStartObject();

                    foreach (var (key, jsonValue) in obj)
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
