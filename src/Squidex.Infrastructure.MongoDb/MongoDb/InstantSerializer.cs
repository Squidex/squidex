// ==========================================================================
//  InstantSerializer.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using NodaTime;

namespace Squidex.Infrastructure.MongoDb
{
    public sealed class InstantSerializer : SerializerBase<Instant>, IBsonPolymorphicSerializer
    {
        private static readonly Lazy<bool> Registerer = new Lazy<bool>(() =>
        {
            BsonSerializer.RegisterSerializer(new InstantSerializer());

            return true;
        });

        public static bool Register()
        {
            return !Registerer.IsValueCreated && Registerer.Value;
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
