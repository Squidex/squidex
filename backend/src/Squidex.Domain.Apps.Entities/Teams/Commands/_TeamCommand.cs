// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Teams.Commands;

public abstract class TeamCommand : TeamCommandBase, ITeamCommand
{
    public DomainId TeamId { get; set; }

    public override DomainId AggregateId
    {
        get => TeamId;
    }
}

// This command is needed as marker for middlewares.
public abstract class TeamCommandBase : SquidexCommand, IAggregateCommand
{
    public abstract DomainId AggregateId { get; }
}
