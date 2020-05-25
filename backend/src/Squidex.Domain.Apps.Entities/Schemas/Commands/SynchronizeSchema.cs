// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands
{
    public sealed class SynchronizeSchema : UpsertCommand, IAggregateCommand, ISchemaCommand
    {
        public NamedId<DomainId> SchemaId { get; set; }

        public bool NoFieldDeletion { get; set; }

        public bool NoFieldRecreation { get; set; }

        DomainId IAggregateCommand.AggregateId
        {
            get { return DomainId.Combine(AppId, SchemaId.Id); }
        }
    }
}
