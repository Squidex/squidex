// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
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
            get { return supportedTypes; }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
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
                        var result = JsonValue.Object();

                        while (reader.Read())
                        {
                            switch (reader.TokenType)
                            {
                                case JsonToken.PropertyName:
                                    var propertyName = reader.Value.ToString();

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
                        var result = JsonValue.Array();

                        while (reader.Read())
                        {
                            switch (reader.TokenType)
                            {
                                case JsonToken.Comment:
                                    break;
                                default:
                                    var value = ReadJson(reader);

                                    result.Add(value);
                                    break;
                                case JsonToken.EndArray:
                                    return result;
                            }
                        }

                        throw new JsonSerializationException("Unexpected end when reading Object.");
                    }

                case JsonToken.Integer:
                    return JsonValue.Create((long)reader.Value);
                case JsonToken.Float:
                    return JsonValue.Create((double)reader.Value);
                case JsonToken.Boolean:
                    return JsonValue.Create((bool)reader.Value);
                case JsonToken.Date:
                    return JsonValue.Create(((DateTime)reader.Value).ToString("yyyy-MM-ddTHH:mm:ssK", CultureInfo.InvariantCulture));
                case JsonToken.String:
                    return JsonValue.Create(reader.Value.ToString());
                case JsonToken.Null:
                case JsonToken.Undefined:
                    return JsonValue.Null;
            }

            throw new NotSupportedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
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
                case JsonNull _:
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

                    foreach (var kvp in obj)
                    {
                        writer.WritePropertyName(kvp.Key);

                        WriteJson(writer, kvp.Value);
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
