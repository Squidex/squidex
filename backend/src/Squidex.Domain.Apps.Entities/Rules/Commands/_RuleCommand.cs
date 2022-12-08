// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Rules.Commands;

public abstract class RuleCommand : RuleCommandBase
{
    public DomainId RuleId { get; set; }

    public override DomainId AggregateId
    {
        get => DomainId.Combine(AppId, RuleId);
    }
}

// This command is needed as marker for middlewares.
public abstract class RuleCommandBase : SquidexCommand, IAppCommand, IAggregateCommand
{
    public NamedId<DomainId> AppId { get; set; }

    public abstract DomainId AggregateId { get; }
}
