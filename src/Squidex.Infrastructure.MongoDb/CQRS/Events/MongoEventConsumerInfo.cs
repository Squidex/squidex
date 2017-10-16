// ==========================================================================
//  MongoEventConsumerInfo.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Squidex.Infrastructure.CQRS.Events
{
    [BsonIgnoreExtraElements]
    public sealed class MongoEventConsumerInfo : IEventConsumerInfo
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Name { get; set; }

        [BsonElement]
        [BsonIgnoreIfNull]
        public string Error { get; set; }

        [BsonElement]
        [BsonIgnoreIfDefault]
        public bool IsStopped { get; set; }

        [BsonElement]
        [BsonRequired]
        public string Position { get; set; }
    }
}