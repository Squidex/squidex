// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Apps.Commands;

public abstract class AppCommand : AppCommandBase, IAppCommand
{
    public NamedId<DomainId> AppId { get; set; }

    public override DomainId AggregateId
    {
        get => AppId.Id;
    }
}

// This command is needed as marker for middlewares.
public abstract class AppCommandBase : SquidexCommand, IAggregateCommand
{
    public abstract DomainId AggregateId { get; }
}
