// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
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
        [BsonRequired]
        [BsonElement]
        public string DocumentId { get; set; }

        [BsonRequired]
        [BsonElement("id")]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonRequired]
        [BsonElement("ai")]
        [BsonRepresentation(BsonType.String)]
        public Guid AppIdId { get; set; }

        [BsonRequired]
        [BsonElement("si")]
        [BsonRepresentation(BsonType.String)]
        public Guid SchemaIdId { get; set; }

        [BsonRequired]
        [BsonElement("rf")]
        [BsonRepresentation(BsonType.String)]
        public List<Guid> ReferencedIds { get; set; }

        [BsonRequired]
        [BsonElement("rd")]
        [BsonRepresentation(BsonType.String)]
        public List<Guid> ReferencedIdsDeleted { get; set; } = new List<Guid>();

        [BsonRequired]
        [BsonElement("st")]
        [BsonRepresentation(BsonType.String)]
        public Status Status { get; set; }

        [BsonRequired]
        [BsonElement("do")]
        [BsonJson]
        public IdContentData DataByIds { get; set; }

        [BsonRequired]
        [BsonElement("ai2")]
        public NamedId<Guid> AppId { get; set; }

        [BsonRequired]
        [BsonElement("si2")]
        public NamedId<Guid> SchemaId { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("sdt")]
        public Status? ScheduledTo { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("sda")]
        public Instant? ScheduledAt { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("sdb")]
        public RefToken ScheduledBy { get; set; }

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
        [BsonElement("dl")]
        public bool IsDeleted { get; set; }

        [BsonRequired]
        [BsonElement("cb")]
        public RefToken CreatedBy { get; set; }

        [BsonRequired]
        [BsonElement("mb")]
        public RefToken LastModifiedBy { get; set; }

        [BsonIgnore]
        public NamedContentData Data
        {
            get { return data; }
        }

        public void ParseData(Schema schema)
        {
            data = DataByIds.ToData(schema, ReferencedIdsDeleted);
        }
    }
}
