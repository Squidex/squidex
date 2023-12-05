// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Squidex.Infrastructure.MongoDb;

public sealed class BsonEscapedDictionarySerializer<TValue, TInstance> : ClassSerializerBase<TInstance> where TInstance : Dictionary<string, TValue?>, new()
{
    private static readonly BsonEscapedDictionarySerializer<TValue, TInstance> Instance = new BsonEscapedDictionarySerializer<TValue, TInstance>();

    public static void Register()
    {
        BsonSerializer.TryRegisterSerializer(Instance);
    }

    private BsonEscapedDictionarySerializer()
    {
    }

    protected override TInstance DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var reader = context.Reader;

        var result = new TInstance();

        reader.ReadStartDocument();

        while (reader.ReadBsonType() != BsonType.EndOfDocument)
        {
            var key = reader.ReadName().BsonToJsonName();

            result.Add(key, BsonSerializer.Deserialize<TValue>(reader));
        }

        reader.ReadEndDocument();

        return result;
    }

    protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, TInstance value)
    {
        var writer = context.Writer;

        writer.WriteStartDocument();

        foreach (var property in value)
        {
            writer.WriteName(property.Key.JsonToBsonName());

            BsonSerializer.Serialize(writer, property.Value);
        }

        writer.WriteEndDocument();
    }
}
