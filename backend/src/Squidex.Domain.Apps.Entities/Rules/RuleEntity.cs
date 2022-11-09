// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules;

public sealed class RuleEntity : IEnrichedRuleEntity
{
    public DomainId Id { get; set; }

    public NamedId<DomainId> AppId { get; set; }

    public NamedId<DomainId> SchemaId { get; set; }

    public long Version { get; set; }

    public Instant Created { get; set; }

    public Instant LastModified { get; set; }

    public RefToken CreatedBy { get; set; }

    public RefToken LastModifiedBy { get; set; }

    public Rule RuleDef { get; set; }

    public bool IsDeleted { get; set; }

    public int NumSucceeded { get; set; }

    public int NumFailed { get; set; }

    public Instant? LastExecuted { get; set; }

    public DomainId UniqueId
    {
        get => DomainId.Combine(AppId, Id);
    }
}
