// ==========================================================================
//  RefTokenSerializer.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

// ReSharper disable InvertIf

namespace Squidex.Infrastructure.MongoDb
{
    public class RefTokenSerializer : SerializerBase<RefToken>
    {
        private static bool isRegistered;
        private static readonly object LockObject = new object();

        public static bool Register()
        {
            if (!isRegistered)
            {
                lock (LockObject)
                {
                    if (!isRegistered)
                    {
                        BsonSerializer.RegisterSerializer(new RefTokenSerializer());

                        isRegistered = true;
                        return true;
                    }
                }
            }

            return false;
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
