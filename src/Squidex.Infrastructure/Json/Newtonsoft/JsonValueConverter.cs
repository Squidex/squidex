// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Infrastructure.Json.Newtonsoft
{
    public sealed class JsonValueConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IJsonValue).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return ReadJson(reader);
        }

        private IJsonValue ReadJson(JsonReader reader)
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

        private void WriteJson(JsonWriter writer, IJsonValue value)
        {
            switch (value)
            {
                case JsonNull n:
                    writer.WriteNull();
                    break;
                case JsonScalar<bool> s:
                    writer.WriteValue(s.Value);
                    break;
                case JsonScalar<string> s:
                    writer.WriteValue(s.Value);
                    break;
                case JsonScalar<double> s:
                    writer.WriteValue(s.Value);
                    break;
                case JsonArray array:
                    {
                        writer.WriteStartArray();

                        foreach (var item in array)
                        {
                            WriteJson(writer, item);
                        }

                        writer.WriteEndArray();
                        break;
                    }

                case JsonObject obj:
                    {
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
        }
    }
}
