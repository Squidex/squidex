// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Newtonsoft.Json.Linq;

namespace Squidex.Infrastructure.MongoDb
{
    public sealed class JTokenSerializer<T> : ClassSerializerBase<T> where T : JToken
    {
        public static readonly JTokenSerializer<T> Instance = new JTokenSerializer<T>();

        protected override T DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var jsonReader = new BsonJsonReader(context.Reader);

            return (T)JToken.ReadFrom(jsonReader);
        }

        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, T value)
        {
            var jsonWriter = new BsonJsonWriter(context.Writer);

            value.WriteTo(jsonWriter);
        }
    }
}
