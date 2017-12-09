// ==========================================================================
//  MongoContentEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    public sealed class MongoContentEntity : IContentEntity
    {
        private NamedContentData data;

        [BsonId]
        [BsonElement]
        public string DocumentId { get; set; }

        [BsonRequired]
        [BsonElement]
        public Guid Id { get; set; }

        [BsonRequired]
        [BsonElement("ct")]
        public Instant Created { get; set; }

        [BsonRequired]
        [BsonElement("mt")]
        public Instant LastModified { get; set; }

        [BsonRequired]
        [BsonElement("dt")]
        public string DataText { get; set; }

        [BsonRequired]
        [BsonElement("vs")]
        public long Version { get; set; }

        [BsonRequired]
        [BsonElement("ai")]
        public Guid AppId { get; set; }

        [BsonRequired]
        [BsonElement("si")]
        public Guid SchemaId { get; set; }

        [BsonRequired]
        [BsonElement("cb")]
        public RefToken CreatedBy { get; set; }

        [BsonRequired]
        [BsonElement("mb")]
        public RefToken LastModifiedBy { get; set; }

        [BsonRequired]
        [BsonElement("rf")]
        public List<Guid> ReferencedIds { get; set; }

        [BsonRequired]
        [BsonElement("rd")]
        public List<Guid> ReferencedIdsDeleted { get; set; } = new List<Guid>();

        [BsonRequired]
        [BsonElement("lt")]
        public bool IsLatest { get; set; }

        [BsonRequired]
        [BsonElement("st")]
        [BsonRepresentation(BsonType.String)]
        public Status Status { get; set; }

        [BsonRequired]
        [BsonElement("do")]
        [BsonJson]
        public IdContentData DataByIds { get; set; }

        NamedContentData IContentEntity.Data
        {
            get { return data; }
        }

        public void ParseData(Schema schema)
        {
            data = DataByIds.ToData(schema, ReferencedIdsDeleted);
        }
    }
}
