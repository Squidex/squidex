// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Schemas.Commands;

public abstract class SchemaCommand : SchemaCommandBase, ISchemaCommand
{
    public NamedId<DomainId> SchemaId { get; set; }

    public override DomainId AggregateId
    {
        get => DomainId.Combine(AppId, SchemaId.Id);
    }
}

// This command is needed as marker for middlewares.
public abstract class SchemaCommandBase : SquidexCommand, IAppCommand, IAggregateCommand
{
    public NamedId<DomainId> AppId { get; set; }

    public abstract DomainId AggregateId { get; }
}
