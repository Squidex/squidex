// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands;

public abstract class SchemaCommand : SchemaCommandBase, ISchemaCommand
{
    public NamedId<DomainId> SchemaId { get; set; }

    public override DomainId AggregateId
    {
        get => DomainId.Combine(AppId, SchemaId.Id);
    }
}
