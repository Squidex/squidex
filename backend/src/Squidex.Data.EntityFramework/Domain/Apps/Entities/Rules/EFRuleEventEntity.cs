// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Rules;

[Table("RuleEvents")]
public sealed class EFRuleEventEntity : IRuleEventEntity
{
    [Key]
    public DomainId Id { get; set; }

    public DomainId AppId { get; set; }

    public DomainId RuleId { get; set; }

    public Instant Created { get; set; }

    public Instant LastModified { get; set; }

    public RuleResult Result { get; set; }

    public RuleJobResult JobResult { get; set; }

    public RuleJob Job { get; set; }

    public string? LastDump { get; set; }

    public int NumCalls { get; set; }

    public Instant Expires { get; set; }

    public Instant? NextAttempt { get; set; }

    public static EFRuleEventEntity FromJob(RuleEventWrite item)
    {
        var (job, nextAttempt, error) = item;

        var entity = new EFRuleEventEntity { Job = job, Id = job.Id, NextAttempt = nextAttempt };

        SimpleMapper.Map(job, entity);

        if (nextAttempt == null)
        {
            entity.JobResult = RuleJobResult.Failed;
            entity.LastDump = error?.ToString();
            entity.LastModified = job.Created;
            entity.Result = RuleResult.Failed;
        }

        return entity;
    }
}
