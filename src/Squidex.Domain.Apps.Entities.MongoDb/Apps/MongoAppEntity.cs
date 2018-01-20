// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Domain.Apps.Entities.Apps.State;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Apps
{
    public sealed class MongoAppEntity : IVersionedEntity<Guid>
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
        public long Version { get; set; }

        [BsonElement]
        [BsonRequired]
        public string Name { get; set; }

        [BsonElement]
        [BsonRequired]
        public string[] UserIds { get; set; }
    }
}
