// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Squidex.Infrastructure.MongoDb
{
    public sealed class DomainIdSerializer : SerializerBase<DomainId>, IBsonPolymorphicSerializer, IRepresentationConfigurable<DomainIdSerializer>
    {
        private static int isRegistered;

        public static void Register()
        {
            if (Interlocked.Increment(ref isRegistered) == 1)
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
        }

        public bool IsDiscriminatorCompatibleWithObjectSerializer
        {
            get { return true; }
        }

        public BsonType Representation { get; } = BsonType.String;

        public override DomainId Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            return DomainId.Create(context.Reader.ReadString());
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
