﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    [BsonIgnoreExtraElements]
    public sealed class MongoContentEntity : IContentEntity, IVersionedEntity<DomainId>
    {
        private NamedContentData data;

        [BsonId]
        [BsonElement("_id")]
        public DomainId DocumentId { get; set; }

        [BsonRequired]
        [BsonElement("_ai")]
        public DomainId IndexedAppId { get; set; }

        [BsonRequired]
        [BsonElement("_si")]
        public DomainId IndexedSchemaId { get; set; }

        [BsonRequired]
        [BsonElement("ai")]
        public NamedId<DomainId> AppId { get; set; }

        [BsonRequired]
        [BsonElement("si")]
        public NamedId<DomainId> SchemaId { get; set; }

        [BsonRequired]
        [BsonElement("rf")]
        public HashSet<DomainId>? ReferencedIds { get; set; }

        [BsonRequired]
        [BsonElement("id")]
        public DomainId Id { get; set; }

        [BsonRequired]
        [BsonElement("ss")]
        public Status Status { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("ns")]
        public Status? NewStatus { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("do")]
        [BsonJson]
        public IdContentData DataByIds { get; set; }

        [BsonIgnoreIfNull]
        [BsonElement("sa")]
        public Instant? ScheduledAt { get; set; }

        [BsonRequired]
        [BsonElement("ct")]
        public Instant Created { get; set; }

        [BsonRequired]
        [BsonElement("mt")]
        public Instant LastModified { get; set; }

        [BsonRequired]
        [BsonElement("vs")]
        public long Version { get; set; }

        [BsonIgnoreIfDefault]
        [BsonElement("dl")]
        public bool IsDeleted { get; set; }

        [BsonIgnoreIfDefault]
        [BsonElement("sj")]
        public ScheduleJob? ScheduleJob { get; set; }

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

        public DomainId UniqueId
        {
            get { return DocumentId; }
        }

        public void LoadData(NamedContentData data, Schema schema, DataConverter converter)
        {
            ReferencedIds = data.GetReferencedIds(schema).Select(x => DomainId.Combine(AppId, x)).ToHashSet();

            DataByIds = converter.ToMongoModel(data, schema);
        }

        public void ParseData(Schema schema, DataConverter converter)
        {
            data = converter.FromMongoModel(DataByIds, schema);
        }
    }
}
