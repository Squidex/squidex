// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using NodaTime;

namespace Squidex.Infrastructure.MongoDb
{
    public sealed class InstantSerializer : SerializerBase<Instant>, IBsonPolymorphicSerializer
    {
        public static void Register()
        {
            try
            {
                BsonSerializer.RegisterSerializer(new InstantSerializer());
            }
            catch (BsonSerializationException)
            {
                return;
            }
        }

        public bool IsDiscriminatorCompatibleWithObjectSerializer
        {
            get => true;
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
