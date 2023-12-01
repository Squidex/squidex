// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Teams.Commands;

public abstract class TeamCommandBase : SquidexCommand, IAggregateCommand
{
    public abstract DomainId AggregateId { get; }
}
