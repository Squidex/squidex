// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Infrastructure.States
{
    [BsonIgnoreExtraElements]
    public sealed class MongoState<T> : IVersionedEntity<DomainId>
    {
        [BsonId]
        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public DomainId DocumentId { get; set; }

        [BsonRequired]
        [BsonElement]
        [BsonJson]
        public T Doc { get; set; }

        [BsonRequired]
        [BsonElement]
        public long Version { get; set; }
    }
}
