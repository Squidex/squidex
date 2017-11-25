// ==========================================================================
//  RefTokenSerializer.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Squidex.Infrastructure.MongoDb
{
    public class JsonBsonSerializer<T> : ClassSerializerBase<T> where T : class
    {
        private readonly JsonSerializer serializer;

        public JsonBsonSerializer(JsonSerializer serializer)
        {
            Guard.NotNull(serializer, nameof(serializer));

            this.serializer = serializer;
        }

        protected override T DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var jsonReader = new BsonJsonReader(context.Reader);

            return serializer.Deserialize<T>(jsonReader);
        }

        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, T value)
        {
            var jsonWriter = new BsonJsonWriter(context.Writer);

            serializer.Serialize(jsonWriter, value);
        }
    }
}
