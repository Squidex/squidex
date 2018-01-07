// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities
{
    public abstract class SchemaAggregateCommand : SchemaCommand, IAggregateCommand
    {
        Guid IAggregateCommand.AggregateId
        {
            get { return SchemaId.Id; }
        }
    }
}
