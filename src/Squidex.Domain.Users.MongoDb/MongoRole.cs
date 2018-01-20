// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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
