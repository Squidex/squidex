// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Squidex.Infrastructure.MongoDb;

public sealed class BsonDomainIdSerializer : SerializerBase<DomainId>, IBsonPolymorphicSerializer, IRepresentationConfigurable<BsonDomainIdSerializer>
{
    private static readonly BsonDomainIdSerializer Instance = new BsonDomainIdSerializer(BsonType.String);

    public static void Register()
    {
        BsonSerializer.TryRegisterSerializer(Instance);
    }

    private BsonDomainIdSerializer()
    {
    }

    public bool IsDiscriminatorCompatibleWithObjectSerializer
    {
        get => true;
    }

    public BsonType Representation { get; }

    public BsonDomainIdSerializer(BsonType representation)
    {
        Representation = representation;
    }

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

                return DomainId.Create(Encoding.UTF8.GetString(binary.Bytes));
            default:
                ThrowHelper.NotSupportedException();
                return default!;
        }
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DomainId value)
    {
        switch (Representation)
        {
            case BsonType.String:
                context.Writer.WriteString(value.ToString());
                break;
            case BsonType.Binary:
                if (Guid.TryParse(value.ToString(), out var guid))
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    if (context.Writer.Settings.GuidRepresentation == GuidRepresentation.CSharpLegacy)
                    {
                        context.Writer.WriteBinaryData(new BsonBinaryData(guid.ToByteArray(), BsonBinarySubType.UuidLegacy, GuidRepresentation.CSharpLegacy));
                    }
                    else
                    {
                        context.Writer.WriteBinaryData(new BsonBinaryData(guid, GuidRepresentation.Standard));
                    }
#pragma warning restore CS0618 // Type or member is obsolete
                }
                else
                {
                    var buffer = Encoding.UTF8.GetBytes(value.ToString());

                    context.Writer.WriteBytes(buffer);
                }

                break;
            default:
                ThrowHelper.NotSupportedException();
                break;
        }
    }

    public BsonDomainIdSerializer WithRepresentation(BsonType representation)
    {
        if (representation is not BsonType.String and not BsonType.Binary)
        {
            ThrowHelper.NotSupportedException();
            return default!;
        }

        if (representation != Representation)
        {
            return new BsonDomainIdSerializer(representation);
        }

        return this;
    }

    IBsonSerializer IRepresentationConfigurable.WithRepresentation(BsonType representation)
    {
        return WithRepresentation(representation);
    }
}
