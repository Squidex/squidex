// ==========================================================================
//  SchemaCreated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Core.Schemas;
using Squidex.Infrastructure;

using SchemaFields = System.Collections.Generic.List<Squidex.Events.Schemas.SchemaCreatedField>;

namespace Squidex.Events.Schemas
{
    [TypeName("SchemaCreatedEvent")]
    public class SchemaCreated : SchemaEvent
    {
        public string Name { get; set; }

        public SchemaFields Fields { get; set; }

        public SchemaProperties Properties { get; set; }
    }
}
