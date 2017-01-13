// ==========================================================================
//  SchemaAggregateCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Write
{
    public abstract class SchemaAggregateCommand : AppCommand, ISchemaCommand
    {
        Guid ISchemaCommand.SchemaId
        {
            get
            {
                return AggregateId;
            }
            set
            {
                AggregateId = value;
            }
        }
    }
}
