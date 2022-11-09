// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Squidex.Infrastructure.MongoDb;

public sealed class BsonDomainIdSerializer : SerializerBase<DomainId>, IBsonPolymorphicSerializer, IRepresentationConfigurable<BsonDomainIdSerializer>
{
    public static void Register()
    {
        try
        {
            BsonSerializer.RegisterSerializer(new BsonDomainIdSerializer());
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

                if (binary.SubType is BsonBinarySubType.UuidLegacy or BsonBinarySubType.UuidStandard)
                {
                    return DomainId.Create(binary.ToGuid());
                }

                return DomainId.Create(binary.ToString());
            default:
                ThrowHelper.NotSupportedException();
                return default!;
        }
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DomainId value)
    {
        context.Writer.WriteString(value.ToString());
    }

    public BsonDomainIdSerializer WithRepresentation(BsonType representation)
    {
        if (representation != BsonType.String)
        {
            ThrowHelper.NotSupportedException();
            return default!;
        }

        return this;
    }

    IBsonSerializer IRepresentationConfigurable.WithRepresentation(BsonType representation)
    {
        return WithRepresentation(representation);
    }
}
