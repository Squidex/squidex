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
using NodaTime.Text;

namespace Squidex.Infrastructure.MongoDb
{
    public sealed class InstantSerializer : SerializerBase<Instant>, IBsonPolymorphicSerializer, IRepresentationConfigurable<InstantSerializer>
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

        public BsonType Representation { get; }

        public InstantSerializer(BsonType representation = BsonType.DateTime)
        {
            if (representation != BsonType.DateTime && representation != BsonType.Int64 && representation != BsonType.String)
            {
                throw new ArgumentException("Unsupported representation.", nameof(representation));
            }

            Representation = representation;
        }

        public override Instant Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var reader = context.Reader;

            switch (reader.CurrentBsonType)
            {
                case BsonType.DateTime:
                    return Instant.FromUnixTimeMilliseconds(context.Reader.ReadDateTime());
                case BsonType.Int64:
                    return Instant.FromUnixTimeMilliseconds(context.Reader.ReadInt64());
                case BsonType.String:
                    return InstantPattern.ExtendedIso.Parse(context.Reader.ReadString()).Value;
            }

            throw new NotSupportedException("Unsupported Representation.");
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Instant value)
        {
            switch (Representation)
            {
                case BsonType.DateTime:
                    context.Writer.WriteDateTime(value.ToUnixTimeMilliseconds());
                    return;
                case BsonType.Int64:
                    context.Writer.WriteInt64(value.ToUnixTimeMilliseconds());
                    return;
                case BsonType.String:
                    context.Writer.WriteString(InstantPattern.ExtendedIso.Format(value));
                    return;
            }

            throw new NotSupportedException("Unsupported Representation.");
        }

        public InstantSerializer WithRepresentation(BsonType representation)
        {
            return Representation == representation ? this : new InstantSerializer(representation);
        }

        IBsonSerializer IRepresentationConfigurable.WithRepresentation(BsonType representation)
        {
            return WithRepresentation(representation);
        }
    }
}
