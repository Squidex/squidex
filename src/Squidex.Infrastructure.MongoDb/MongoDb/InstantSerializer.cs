// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using NodaTime;

namespace Squidex.Infrastructure.MongoDb
{
    public sealed class InstantSerializer : SerializerBase<Instant>, IBsonPolymorphicSerializer
    {
        private static volatile int isRegistered;

        public static void Register()
        {
            if (Interlocked.Increment(ref isRegistered) == 1)
            {
                BsonSerializer.RegisterSerializer(new InstantSerializer());
            }
        }

        public bool IsDiscriminatorCompatibleWithObjectSerializer
        {
            get { return true; }
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
