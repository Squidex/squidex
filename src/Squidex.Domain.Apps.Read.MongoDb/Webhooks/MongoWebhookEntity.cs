// ==========================================================================
//  MongoWebhookEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using Squidex.Domain.Apps.Core.Webhooks;
using Squidex.Domain.Apps.Read.Webhooks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Read.MongoDb.Webhooks
{
    public class MongoWebhookEntity : MongoEntity, IWebhookEntity
    {
        [BsonRequired]
        [BsonElement]
        public Uri Url { get; set; }

        [BsonRequired]
        [BsonElement]
        public Guid AppId { get; set; }

        [BsonRequired]
        [BsonElement]
        public long Version { get; set; }

        [BsonRequired]
        [BsonElement]
        public RefToken CreatedBy { get; set; }

        [BsonRequired]
        [BsonElement]
        public RefToken LastModifiedBy { get; set; }

        [BsonRequired]
        [BsonElement]
        public string SharedSecret { get; set; }

        [BsonRequired]
        [BsonElement]
        public long TotalSucceeded { get; set; }

        [BsonRequired]
        [BsonElement]
        public long TotalFailed { get; set; }

        [BsonRequired]
        [BsonElement]
        public long TotalTimedout { get; set; }

        [BsonRequired]
        [BsonElement]
        public long TotalRequestTime { get; set; }

        [BsonRequired]
        [BsonElement]
        public List<WebhookSchema> Schemas { get; set; }

        [BsonRequired]
        [BsonElement]
        public List<Guid> SchemaIds { get; set; }

        IEnumerable<WebhookSchema> IWebhookEntity.Schemas
        {
            get { return Schemas; }
        }
    }
}
