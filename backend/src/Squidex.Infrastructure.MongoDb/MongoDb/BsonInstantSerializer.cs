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

namespace Squidex.Infrastructure.MongoDb;

public sealed class BsonInstantSerializer : SerializerBase<Instant>, IBsonPolymorphicSerializer, IRepresentationConfigurable<BsonInstantSerializer>
{
    public static void Register()
    {
        try
        {
            BsonSerializer.RegisterSerializer(new BsonInstantSerializer());
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

    public BsonInstantSerializer()
        : this(BsonType.DateTime)
    {
    }

    public BsonInstantSerializer(BsonType representation)
    {
        if (representation is not BsonType.DateTime and not BsonType.Int64 and not BsonType.String)
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
            default:
                ThrowHelper.NotSupportedException("Unsupported Representation.");
                return default!;
        }
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Instant value)
    {
        switch (Representation)
        {
            case BsonType.DateTime:
                context.Writer.WriteDateTime(value.ToUnixTimeMilliseconds());
                break;
            case BsonType.Int64:
                context.Writer.WriteInt64(value.ToUnixTimeMilliseconds());
                break;
            case BsonType.String:
                context.Writer.WriteString(InstantPattern.ExtendedIso.Format(value));
                break;
            default:
                ThrowHelper.NotSupportedException("Unsupported Representation.");
                break;
        }
    }

    public BsonInstantSerializer WithRepresentation(BsonType representation)
    {
        return Representation == representation ? this : new BsonInstantSerializer(representation);
    }

    IBsonSerializer IRepresentationConfigurable.WithRepresentation(BsonType representation)
    {
        return WithRepresentation(representation);
    }
}
