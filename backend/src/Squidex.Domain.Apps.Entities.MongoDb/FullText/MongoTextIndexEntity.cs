// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.MongoDb.FullText
{
    public sealed class MongoTextIndexEntity
    {
        [BsonId]
        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public string DocId { get; set; }

        [BsonRequired]
        [BsonElement("_ci")]
        [BsonRepresentation(BsonType.String)]
        public DomainId ContentId { get; set; }

        [BsonRequired]
        [BsonElement("_ai")]
        [BsonRepresentation(BsonType.String)]
        public DomainId AppId { get; set; }

        [BsonRequired]
        [BsonElement("_si")]
        [BsonRepresentation(BsonType.String)]
        public DomainId SchemaId { get; set; }

        [BsonRequired]
        [BsonElement("fa")]
        public bool ServeAll { get; set; }

        [BsonRequired]
        [BsonElement("fp")]
        public bool ServePublished { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("t")]
        public List<MongoTextIndexEntityText> Texts { get; set; }
    }
}
