// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Squidex.Infrastructure.MongoDb
{
    public class RefTokenSerializer : ClassSerializerBase<RefToken>
    {
        private static readonly Lazy<bool> Registerer = new Lazy<bool>(() =>
        {
            BsonSerializer.RegisterSerializer(new RefTokenSerializer());

            return true;
        });

        public static bool Register()
        {
            return !Registerer.IsValueCreated && Registerer.Value;
        }

        protected override RefToken DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var value = context.Reader.ReadString();

            return RefToken.Parse(value);
        }

        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, RefToken value)
        {
            context.Writer.WriteString(value.ToString());
        }
    }
}
