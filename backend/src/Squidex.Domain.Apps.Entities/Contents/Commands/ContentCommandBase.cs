// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Contents.Commands;

public abstract class ContentCommandBase : SquidexCommand, IAppCommand, ISchemaCommand, IAggregateCommand
{
    public NamedId<DomainId> AppId { get; set; }

    public NamedId<DomainId> SchemaId { get; set; }

    public abstract DomainId AggregateId { get; }
}
