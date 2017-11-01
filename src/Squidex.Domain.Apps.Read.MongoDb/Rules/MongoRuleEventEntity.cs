// ==========================================================================
//  MongoRuleEventEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Read.Rules;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Read.MongoDb.Rules
{
    public sealed class MongoRuleEventEntity : MongoEntity, IRuleEventEntity
    {
        [BsonRequired]
        [BsonElement]
        public Guid AppId { get; set; }

        [BsonRequired]
        [BsonElement]
        public string LastDump { get; set; }

        [BsonRequired]
        [BsonElement]
        public int NumCalls { get; set; }

        [BsonRequired]
        [BsonElement]
        public Instant Expires { get; set; }

        [BsonRequired]
        [BsonElement]
        public Instant? NextAttempt { get; set; }

        [BsonRequired]
        [BsonElement]
        public bool IsSending { get; set; }

        [BsonRequired]
        [BsonElement]
        public RuleResult Result { get; set; }

        [BsonRequired]
        [BsonElement]
        public RuleJobResult JobResult { get; set; }

        [BsonRequired]
        [BsonElement]
        [BsonJson]
        public RuleJob Job { get; set; }
    }
}
