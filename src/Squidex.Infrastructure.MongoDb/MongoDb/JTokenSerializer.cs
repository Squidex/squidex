// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Newtonsoft.Json.Linq;

namespace Squidex.Infrastructure.MongoDb
{
    public sealed class JTokenSerializer<T> : ClassSerializerBase<T> where T : JToken
    {
        public static readonly JTokenSerializer<T> Instance = new JTokenSerializer<T>();

        public override T Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            if (bsonReader.GetCurrentBsonType() == BsonType.Null)
            {
                bsonReader.ReadNull();

                return null;
            }
            else
            {
                var jsonReader = new BsonJsonReader(bsonReader);

                return (T)JToken.ReadFrom(jsonReader);
            }
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, T value)
        {
            var bsonWriter = context.Writer;

            if (value == null)
            {
                bsonWriter.WriteNull();
            }
            else
            {
                var jsonWriter = new BsonJsonWriter(bsonWriter);

                value.WriteTo(jsonWriter);
            }
        }
    }
}
