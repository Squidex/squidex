// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;

namespace Squidex.Infrastructure.MongoDb
{
    public abstract class MongoEntity
    {
        [BsonId]
        [BsonElement]
        public string Id { get; set; }

        [BsonRequired]
        [BsonElement]
        public Instant Created { get; set; }

        [BsonRequired]
        [BsonElement]
        public Instant LastModified { get; set; }
    }
}
