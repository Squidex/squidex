// ==========================================================================
//  MongoRuleEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Domain.Apps.Entities.Rules.State;

namespace Squidex.Domain.Apps.Entities.MongoDb.Rules
{
    public sealed class MongoRuleEntity
    {
        [BsonId]
        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; }

        [BsonElement]
        [BsonRequired]
        public RuleState State { get; set; }

        [BsonElement]
        [BsonRequired]
        public int Version { get; set; }
    }
}
