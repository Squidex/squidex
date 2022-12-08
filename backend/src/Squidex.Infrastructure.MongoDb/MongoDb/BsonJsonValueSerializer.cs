// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Infrastructure.MongoDb;

public sealed class BsonJsonValueSerializer : SerializerBase<JsonValue>
{
    public static void Register()
    {
        try
        {
            BsonSerializer.RegisterSerializer(new BsonJsonValueSerializer());
        }
        catch (BsonSerializationException)
        {
            return;
        }
    }

    public override JsonValue Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var reader = context.Reader;

        switch (reader.CurrentBsonType)
        {
            case BsonType.Undefined:
                reader.ReadUndefined();
                return JsonValue.Null;
            case BsonType.Null:
                reader.ReadNull();
                return JsonValue.Null;
            case BsonType.Boolean:
                return reader.ReadBoolean();
            case BsonType.Double:
                return reader.ReadDouble();
            case BsonType.Int32:
                return reader.ReadInt32();
            case BsonType.Int64:
                return reader.ReadInt64();
            case BsonType.String:
                return reader.ReadString();
            case BsonType.Array:
                return BsonSerializer.Deserialize<JsonArray>(reader);
            case BsonType.Document:
                return BsonSerializer.Deserialize<JsonObject>(reader);
            default:
                ThrowHelper.NotSupportedException("Unsupported Representation.");
                return default!;
        }
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, JsonValue value)
    {
        var writer = context.Writer;

        switch (value.Value)
        {
            case null:
                writer.WriteNull();
                break;
            case bool b:
                writer.WriteBoolean(b);
                break;
            case string s:
                writer.WriteString(s);
                break;
            case double n:
                writer.WriteDouble(n);
                break;
            case JsonArray a:
                BsonSerializer.Serialize(writer, a);
                break;
            case JsonObject o:
                BsonSerializer.Serialize(writer, o);
                break;
            default:
                ThrowHelper.NotSupportedException();
                break;
        }
    }
}
