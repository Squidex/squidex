// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.TestHelpers;

namespace Squidex.TestHelpers;

public static class MongoTestUtils
{
    public sealed class ObjectHolder<T>
    {
        [BsonRequired]
        public T Value1 { get; set; }

        [BsonRequired]
        public T Value2 { get; set; }
    }

    static MongoTestUtils()
    {
        SetupBson();
    }

    public static void SetupBson()
    {
        BsonDefaultConventions.Register();
        BsonDomainIdSerializer.Register();
        BsonEscapedDictionarySerializer<JsonValue, JsonObject>.Register();
        BsonInstantSerializer.Register();
        BsonJsonConvention.Register(TestUtils.DefaultOptions());
        BsonJsonValueSerializer.Register();
        BsonStringSerializer<RefToken>.Register();
    }

    public static T SerializeAndDeserializeBson<T>(this T value)
    {
        return SerializeAndDeserializeBson<T, T>(value);
    }

    public static TOut SerializeAndDeserializeBson<TOut, TIn>(this TIn value)
    {
        using var stream = new MemoryStream();

        using (var writer = new BsonBinaryWriter(stream))
        {
            BsonSerializer.Serialize(writer, new ObjectHolder<TIn> { Value1 = value, Value2 = value });
        }

        stream.Position = 0;

        using (var reader = new BsonBinaryReader(stream))
        {
            return BsonSerializer.Deserialize<ObjectHolder<TOut>>(reader).Value1;
        }
    }
}
