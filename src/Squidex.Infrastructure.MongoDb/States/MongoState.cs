// ==========================================================================
//  MongoState.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Infrastructure.States
{
    public sealed class MongoState<T, TKey>
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
