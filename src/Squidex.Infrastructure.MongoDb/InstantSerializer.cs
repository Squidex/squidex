// ==========================================================================
//  InstantSerializer.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using NodaTime;
using NodaTime.Text;

// ReSharper disable InvertIf

namespace Squidex.Infrastructure.MongoDb
{
    public sealed class InstantSerializer : SerializerBase<Instant>
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
                        BsonSerializer.RegisterSerializer(new InstantSerializer());

                        isRegistered = true;
                        return true;
                    }
                }
            }

            return false;
        }

        public override Instant Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var value = context.Reader.ReadDateTime();

            return Instant.FromUnixTimeMilliseconds(value);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Instant value)
        {
            context.Writer.WriteDateTime(value.ToUnixTimeMilliseconds());
        }
    }
}
