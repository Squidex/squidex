// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Identity;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Squidex.Domain.Users;

internal sealed class IdentityUserForwardingSerializer
    : SerializerBase<IdentityUser>, IBsonDocumentSerializer
{
    private static readonly IBsonSerializer<MongoUser> MongoUserSerializer =
        BsonSerializer.LookupSerializer<MongoUser>();

    public override IdentityUser Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        args.NominalType = typeof(MongoUser);
        return MongoUserSerializer.Deserialize(context, args);
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, IdentityUser value)
    {
        args.NominalType = typeof(MongoUser);
        MongoUserSerializer.Serialize(context, args, (MongoUser)value);
    }

    public bool TryGetMemberSerializationInfo(string memberName, out BsonSerializationInfo serializationInfo)
    {
        return ((IBsonDocumentSerializer)MongoUserSerializer)
            .TryGetMemberSerializationInfo(memberName, out serializationInfo);
    }
}
