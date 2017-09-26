// ==========================================================================
//  RefTokenSerializer.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Squidex.Infrastructure.MongoDb
{
    public class RefTokenSerializer : SerializerBase<RefToken>
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

        public override RefToken Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var value = context.Reader.ReadString();

            return value != null ? RefToken.Parse(value) : null;
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, RefToken value)
        {
            if (value != null)
            {
                context.Writer.WriteString(value.ToString());
            }
            else
            {
                context.Writer.WriteNull();
            }
        }
    }
}
