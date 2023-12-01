// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules.Commands;

public abstract class RuleCommand : RuleCommandBase
{
    public DomainId RuleId { get; set; }

    public override DomainId AggregateId
    {
        get => DomainId.Combine(AppId, RuleId);
    }
}
