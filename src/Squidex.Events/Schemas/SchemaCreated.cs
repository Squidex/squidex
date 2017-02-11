// ==========================================================================
//  SchemaCreated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Events.Schemas
{
    [TypeName("SchemaCreatedEvent")]
    public class SchemaCreated : SchemaEvent
    {
        public string Name { get; set; }

        public SchemaProperties Properties { get; set; }
    }
}
