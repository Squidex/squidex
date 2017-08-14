// ==========================================================================
//  MongoWebhookEventEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;
using Squidex.Domain.Apps.Read.Webhooks;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Read.MongoDb.Webhooks
{
    public sealed class MongoWebhookEventEntity : MongoEntity, IWebhookEventEntity
    {
        private WebhookJob job;

        [BsonRequired]
        [BsonElement]
        public Guid AppId { get; set; }

        [BsonRequired]
        [BsonElement]
        public long Version { get; set; }

        [BsonRequired]
        [BsonElement]
        public Uri RequestUrl { get; set; }

        [BsonRequired]
        [BsonElement]
        public string RequestBody { get; set; }

        [BsonRequired]
        [BsonElement]
        public string RequestSignature { get; set; }

        [BsonRequired]
        [BsonElement]
        public string EventName { get; set; }

        [BsonRequired]
        [BsonElement]
        public string LastDump { get; set; }

        [BsonRequired]
        [BsonElement]
        public Instant Expires { get; set; }

        [BsonRequired]
        [BsonElement]
        public Instant? NextAttempt { get; set; }

        [BsonRequired]
        [BsonElement]
        public int NumCalls { get; set; }

        [BsonRequired]
        [BsonElement]
        public bool IsSending { get; set; }

        [BsonRequired]
        [BsonElement]
        public WebhookResult Result { get; set; }

        [BsonRequired]
        [BsonElement]
        public WebhookJobResult JobResult { get; set; }

        public WebhookJob Job
        {
            get { return job ?? (job = SimpleMapper.Map(this, new WebhookJob())); }
        }
    }
}
