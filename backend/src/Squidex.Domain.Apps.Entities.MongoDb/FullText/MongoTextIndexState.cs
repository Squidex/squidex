// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;

namespace Squidex.Domain.Apps.Entities.MongoDb.FullText
{
    public sealed class MongoTextIndexState
    {
        [BsonId]
        [BsonElement]
        public string DocumentId { get; set; }

        [BsonRequired]
        [BsonElement]
        public string ContentId { get; set; }

        [BsonRequired]
        [BsonElement("c")]
        public string DocIdCurrent { get; set; }

        [BsonRequired]
        [BsonElement("n")]
        public string? DocIdNew { get; set; }

        [BsonRequired]
        [BsonElement("p")]
        public string? DocIdForPublished { get; set; }
    }
}
