// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands
{
    public abstract class SchemaUpdateCommand : SchemaCommand, ISchemaCommand
    {
        public NamedId<DomainId> SchemaId { get; set; }

        public override DomainId AggregateId
        {
            get { return DomainId.Combine(AppId, SchemaId.Id); }
        }
    }
}
