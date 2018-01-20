// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using MongoDB.Bson.Serialization.Attributes;

namespace Squidex.Domain.Users.MongoDb
{
    public sealed class MongoUserClaim
    {
        [BsonRequired]
        [BsonElement]
        public string Type { get; set; }

        [BsonRequired]
        [BsonElement]
        public string Value { get; set; }

        public static implicit operator MongoUserClaim(Claim claim)
        {
            return new MongoUserClaim { Type = claim.Type, Value = claim.Value };
        }

        public static implicit operator Claim(MongoUserClaim userClaim)
        {
            return new Claim(userClaim.Type, userClaim.Value);
        }
    }
}
