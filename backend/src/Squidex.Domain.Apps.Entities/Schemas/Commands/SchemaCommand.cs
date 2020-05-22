// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands
{
    public abstract class SchemaCommand : SquidexCommand, IAggregateCommand
    {
        public DomainId SchemaId { get; set; }

        DomainId IAggregateCommand.AggregateId
        {
            get { return SchemaId; }
        }
    }
}
