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
    public class JsonBsonSerializer : ClassSerializerBase<object>
    {
        private readonly JsonSerializer serializer;

        public JsonBsonSerializer(JsonSerializer serializer)
        {
            Guard.NotNull(serializer, nameof(serializer));

            this.serializer = serializer;
        }

        protected override object DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            return BsonSerializer.Deserialize<BsonDocument>(context.Reader).ToJson().ToObject(args.NominalType, serializer);
        }

        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            BsonSerializer.Serialize(context.Writer, JObject.FromObject(value, serializer).ToBson());
        }
    }
}
