// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Squidex.Infrastructure.ObjectPool;

namespace Squidex.Infrastructure.MongoDb
{
    public sealed class BsonJsonSerializer<T> : ClassSerializerBase<T?> where T : class
    {
        public override T? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            if (bsonReader.GetCurrentBsonType() == BsonType.Null)
            {
                bsonReader.ReadNull();
                return null;
            }

            using var stream = DefaultPools.MemoryStream.GetStream();

            using (var writer = new Utf8JsonWriter(stream))
            {
                FromBson(bsonReader, writer);
            }

            stream.Position = 0;

            return JsonSerializer.Deserialize<T>(stream, BsonJsonConvention.Options);
        }

        private static void FromBson(IBsonReader reader, Utf8JsonWriter writer)
        {
            void ReadDocument()
            {
                reader.ReadStartDocument();
                writer.WriteStartObject();

                while (reader.ReadBsonType() != BsonType.EndOfDocument)
                {
                    Read();
                }

                writer.WriteEndObject();
                reader.ReadEndDocument();
            }

            void ReadArray()
            {
                reader.ReadStartArray();
                writer.WriteStartArray();

                while (reader.ReadBsonType() != BsonType.EndOfDocument)
                {
                    Read();
                }

                writer.WriteEndArray();
                reader.ReadEndArray();
            }

            void Read()
            {
                switch (reader.State)
                {
                    case BsonReaderState.Initial:
                    case BsonReaderState.Type:
                        reader.ReadBsonType();
                        Read();
                        break;
                    case BsonReaderState.Name:
                        writer.WritePropertyName(reader.ReadName().UnescapeBson());
                        Read();
                        break;
                    case BsonReaderState.Value:
                        switch (reader.CurrentBsonType)
                        {
                            case BsonType.Null:
                                reader.ReadNull();
                                writer.WriteNullValue();
                                break;
                            case BsonType.Binary:
                                var valueBinary = reader.ReadBinaryData();
                                writer.WriteBase64StringValue(valueBinary.Bytes.AsSpan());
                                break;
                            case BsonType.Boolean:
                                var valueBoolean = reader.ReadBoolean();
                                writer.WriteBooleanValue(valueBoolean);
                                break;
                            case BsonType.Int32:
                                var valueInt32 = reader.ReadInt32();
                                writer.WriteNumberValue(valueInt32);
                                break;
                            case BsonType.Int64:
                                var valueInt64 = reader.ReadInt64();
                                writer.WriteNumberValue(valueInt64);
                                break;
                            case BsonType.Double:
                                var valueDouble = reader.ReadDouble();
                                writer.WriteNumberValue(valueDouble);
                                break;
                            case BsonType.String:
                                var valueString = reader.ReadString();
                                writer.WriteStringValue(valueString);
                                break;
                            case BsonType.Array:
                                ReadArray();
                                break;
                            case BsonType.Document:
                                ReadDocument();
                                break;
                            default:
                                throw new NotSupportedException();
                        }

                        break;
                    case BsonReaderState.Done:
                        break;
                    case BsonReaderState.Closed:
                        break;
                }
            }

            Read();
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, T? value)
        {
            var bsonWriter = context.Writer;

            using (var jsonDocument = JsonSerializer.SerializeToDocument(value, BsonJsonConvention.Options))
            {
                WriteElement(bsonWriter, jsonDocument.RootElement);
            }
        }

        private static void WriteElement(IBsonWriter writer, JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Null:
                    writer.WriteNull();
                    break;
                case JsonValueKind.String:
                    writer.WriteString(element.GetString());
                    break;
                case JsonValueKind.Number:
                    writer.WriteDouble(element.GetDouble());
                    break;
                case JsonValueKind.True:
                    writer.WriteBoolean(true);
                    break;
                case JsonValueKind.False:
                    writer.WriteBoolean(false);
                    break;
                case JsonValueKind.Array:
                    writer.WriteStartArray();

                    foreach (var item in element.EnumerateArray())
                    {
                        WriteElement(writer, item);
                    }

                    writer.WriteEndArray();
                    break;
                case JsonValueKind.Object:
                    writer.WriteStartDocument();

                    foreach (var property in element.EnumerateObject())
                    {
                        writer.WriteName(property.Name.EscapeJson());
                        WriteElement(writer, property.Value);
                    }

                    writer.WriteEndDocument();
                    break;
                default:
                    ThrowHelper.NotSupportedException();
                    break;
            }
        }
    }
}
