// ==========================================================================
//  SchemaCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Write
{
    public abstract class SchemaCommand : AppCommand
    {
        public NamedId<Guid> SchemaId { get; set; }
    }
}
