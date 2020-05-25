// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    [BsonIgnoreExtraElements]
    public sealed class MongoContentEntity : IContentEntity, IVersionedEntity<string>
    {
        private NamedContentData data;

        [BsonId]
        [BsonElement("_id")]
        public string DocumentId { get; set; }

        [BsonRequired]
        [BsonElement("_ai")]
        public string IndexedAppId { get; set; }

        [BsonRequired]
        [BsonElement("_si")]
        public string IndexedSchemaId { get; set; }

        [BsonRequired]
        [BsonElement("rf")]
        public HashSet<string>? ReferencedIds { get; set; }

        [BsonRequired]
        [BsonElement("ci")]
        public string ContentId { get; set; }

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

        [BsonRequired]
        [BsonElement("ai")]
        public NamedId<DomainId> AppId { get; set; }

        [BsonRequired]
        [BsonElement("si")]
        public NamedId<DomainId> SchemaId { get; set; }

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

        DomainId IEntity.Id
        {
            get { return ContentId; }
        }

        public void LoadData(NamedContentData data, Schema schema, DataConverter converter)
        {
            DataByIds = converter.ToMongoModel(data, schema);
        }

        public void ParseData(Schema schema, DataConverter converter)
        {
            data = converter.FromMongoModel(DataByIds, schema);
        }
    }
}
