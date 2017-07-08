// ==========================================================================
//  SchemaAggregateCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure.CQRS.Commands;

namespace Squidex.Write
{
    public abstract class SchemaAggregateCommand : SchemaCommand, IAggregateCommand
    {
        Guid IAggregateCommand.AggregateId
        {
            get { return SchemaId.Id; }
        }
    }
}
