// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.MongoDb.Rules;

public sealed class MongoRuleEventEntity : IRuleEventEntity
{
    [BsonId]
    [BsonElement("_id")]
    public DomainId JobId { get; set; }

    [BsonRequired]
    [BsonElement(nameof(AppId))]
    public DomainId AppId { get; set; }

    [BsonIgnoreIfDefault]
    [BsonElement(nameof(RuleId))]
    public DomainId RuleId { get; set; }

    [BsonRequired]
    [BsonElement(nameof(Created))]
    public Instant Created { get; set; }

    [BsonRequired]
    [BsonElement(nameof(LastModified))]
    public Instant LastModified { get; set; }

    [BsonRequired]
    [BsonElement(nameof(Result))]
    [BsonRepresentation(BsonType.String)]
    public RuleResult Result { get; set; }

    [BsonRequired]
    [BsonElement(nameof(JobResult))]
    [BsonRepresentation(BsonType.String)]
    public RuleJobResult JobResult { get; set; }

    [BsonRequired]
    [BsonElement(nameof(Job))]
    [BsonJson]
    public RuleJob Job { get; set; }

    [BsonRequired]
    [BsonElement(nameof(LastDump))]
    public string? LastDump { get; set; }

    [BsonRequired]
    [BsonElement(nameof(NumCalls))]
    public int NumCalls { get; set; }

    [BsonRequired]
    [BsonElement(nameof(Expires))]
    public Instant Expires { get; set; }

    [BsonRequired]
    [BsonElement(nameof(NextAttempt))]
    public Instant? NextAttempt { get; set; }

    DomainId IWithId<DomainId>.Id
    {
        get => JobId;
    }

    DomainId IEntity.UniqueId
    {
        get => JobId;
    }

    public static MongoRuleEventEntity FromJob(RuleJob job, Instant? nextAttempt)
    {
        var entity = new MongoRuleEventEntity
        {
            Job = job,
            JobId = job.Id,
            NextAttempt = nextAttempt
        };

        SimpleMapper.Map(job, entity);

        return entity;
    }
}
