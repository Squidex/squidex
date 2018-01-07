// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Rules
{
    public sealed class MongoRuleEventEntity : MongoEntity, IRuleEventEntity
    {
        [BsonRequired]
        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public Guid AppId { get; set; }

        [BsonRequired]
        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public RuleResult Result { get; set; }

        [BsonRequired]
        [BsonElement]
        [BsonRepresentation(BsonType.String)]
        public RuleJobResult JobResult { get; set; }

        [BsonRequired]
        [BsonElement]
        [BsonJson]
        public RuleJob Job { get; set; }

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
    }
}
