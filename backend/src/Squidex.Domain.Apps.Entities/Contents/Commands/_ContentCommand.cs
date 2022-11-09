// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Contents.Commands;

public abstract class ContentCommand : ContentCommandBase
{
    public DomainId ContentId { get; set; }

    public bool DoNotScript { get; set; }

    public override DomainId AggregateId
    {
        get => DomainId.Combine(AppId, ContentId);
    }
}

public abstract class ContentCommandBase : SquidexCommand, IAppCommand, ISchemaCommand, IAggregateCommand
{
    public NamedId<DomainId> AppId { get; set; }

    public NamedId<DomainId> SchemaId { get; set; }

    public abstract DomainId AggregateId { get; }
}
