// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Newtonsoft.Json;

namespace Squidex.Infrastructure.MongoDb
{
    public sealed class BsonJsonSerializer<T> : ClassSerializerBase<T?> where T : class
    {
        private readonly JsonSerializer serializer;

        public BsonJsonSerializer(JsonSerializer serializer)
        {
            Guard.NotNull(serializer, nameof(serializer));

            this.serializer = serializer;
        }

        public override T? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
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

                return serializer.Deserialize<T>(jsonReader);
            }
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, T? value)
        {
            var bsonWriter = context.Writer;

            if (value == null)
            {
                bsonWriter.WriteNull();
            }
            else
            {
                var jsonWriter = new BsonJsonWriter(bsonWriter);

                serializer.Serialize(jsonWriter, value);
            }
        }
    }
}
