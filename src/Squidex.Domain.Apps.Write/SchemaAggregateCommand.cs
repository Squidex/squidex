// ==========================================================================
//  SchemaAggregateCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Write
{
    public abstract class SchemaAggregateCommand : SchemaCommand, IAggregateCommand
    {
        Guid IAggregateCommand.AggregateId
        {
            get { return SchemaId.Id; }
        }
    }
}
