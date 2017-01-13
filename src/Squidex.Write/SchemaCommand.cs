// ==========================================================================
//  SchemaCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Write
{
    public abstract class SchemaCommand : AppCommand, ISchemaCommand
    {
        public Guid SchemaId { get; set; }
    }
}
