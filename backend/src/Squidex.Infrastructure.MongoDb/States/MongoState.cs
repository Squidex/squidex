// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Infrastructure.States
{
    [BsonIgnoreExtraElements]
    public sealed class MongoState<T, TKey> : IVersionedEntity<TKey>
    {
        [BsonId]
        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public TKey Id { get; set; }

        [BsonRequired]
        [BsonElement]
        [BsonJson]
        public T Doc { get; set; }

        [BsonRequired]
        [BsonElement]
        public long Version { get; set; }
    }
}
