// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands
{
    public abstract class SchemaCommand : SquidexCommand, IAggregateCommand
    {
        public Guid SchemaId { get; set; }

        Guid IAggregateCommand.AggregateId
        {
            get { return SchemaId; }
        }
    }
}
