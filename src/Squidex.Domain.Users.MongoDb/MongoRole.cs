// ==========================================================================
//  MongoRole.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Squidex.Domain.Users.MongoDb
{
    public sealed class MongoRole : IRole
    {
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement]
        public string Id { get; set; }

        [BsonRequired]
        [BsonElement]
        public string Name { get; set; }

        [BsonRequired]
        [BsonElement]
        public string NormalizedName { get; set; }
    }
}
