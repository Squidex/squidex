// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;

namespace Squidex.Domain.Users.MongoDb
{
    public sealed class MongoUserToken
    {
        [BsonRequired]
        [BsonElement]
        public string LoginProvider { get; set; }

        [BsonIgnoreIfDefault]
        [BsonElement]
        public string Name { get; set; }

        [BsonRequired]
        [BsonElement]
        public string Value { get; set; }
    }
}
