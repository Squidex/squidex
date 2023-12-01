// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

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
