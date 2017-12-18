// ==========================================================================
//  MongoAppEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Domain.Apps.Entities.Apps.State;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Apps
{
    public sealed class MongoAppEntity
    {
        [BsonId]
        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonElement]
        [BsonRequired]
        [BsonJson]
        public AppState State { get; set; }

        [BsonElement]
        [BsonRequired]
        public int Version { get; set; }

        [BsonElement]
        [BsonRequired]
        public string Name { get; set; }

        [BsonElement]
        [BsonRequired]
        public string[] UserIds { get; set; }
    }
}
