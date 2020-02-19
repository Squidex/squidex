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
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    [BsonIgnoreExtraElements]
    public sealed class MongoContentEntity : IContentEntity, IVersionedEntity<Guid>
    {
        private NamedContentData data;

        [BsonId]
        [BsonElement("_id")]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonRequired]
        [BsonElement("_ai")]
        [BsonRepresentation(BsonType.String)]
        public Guid IndexedAppId { get; set; }

        [BsonRequired]
        [BsonElement("_si")]
        [BsonRepresentation(BsonType.String)]
        public Guid IndexedSchemaId { get; set; }

        [BsonRequired]
        [BsonElement("rf")]
        [BsonRepresentation(BsonType.String)]
        public HashSet<Guid>? ReferencedIds { get; set; }

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
        public NamedId<Guid> AppId { get; set; }

        [BsonRequired]
        [BsonElement("si")]
        public NamedId<Guid> SchemaId { get; set; }

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

        public void LoadData(NamedContentData data, Schema schema, IJsonSerializer serializer)
        {
            ReferencedIds = data.GetReferencedIds(schema);

            DataByIds = data.ToMongoModel(schema, serializer);
        }

        public void ParseData(Schema schema, IJsonSerializer serializer)
        {
            data = DataByIds.FromMongoModel(schema, serializer);
        }
    }
}
