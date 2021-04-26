// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Squidex.Infrastructure.MongoDb
{
    public sealed class DomainIdSerializer : SerializerBase<DomainId>, IBsonPolymorphicSerializer, IRepresentationConfigurable<DomainIdSerializer>
    {
        public static void Register()
        {
            try
            {
                BsonSerializer.RegisterSerializer(new DomainIdSerializer());
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

        public BsonType Representation { get; } = BsonType.String;

        public override DomainId Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            switch (context.Reader.CurrentBsonType)
            {
                case BsonType.String:
                    return DomainId.Create(context.Reader.ReadString());
                case BsonType.Binary:
                    var binary = context.Reader.ReadBinaryData();

                    if (binary.SubType == BsonBinarySubType.UuidLegacy ||
                        binary.SubType == BsonBinarySubType.UuidStandard)
                    {
                        return DomainId.Create(binary.ToGuid());
                    }

                    return DomainId.Create(binary.ToString());
                default:
                    throw new NotSupportedException();
            }
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DomainId value)
        {
            context.Writer.WriteString(value.ToString());
        }

        public DomainIdSerializer WithRepresentation(BsonType representation)
        {
            if (representation != BsonType.String)
            {
                throw new NotSupportedException();
            }

            return this;
        }

        IBsonSerializer IRepresentationConfigurable.WithRepresentation(BsonType representation)
        {
            return WithRepresentation(representation);
        }
    }
}
