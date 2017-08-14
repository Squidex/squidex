// ==========================================================================
//  SchemaCreated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using SchemaFields = System.Collections.Generic.List<Squidex.Domain.Apps.Events.Schemas.SchemaCreatedField>;

namespace Squidex.Domain.Apps.Events.Schemas
{
    [TypeName("SchemaCreatedEvent")]
    public sealed class SchemaCreated : SchemaEvent
    {
        public string Name { get; set; }

        public SchemaFields Fields { get; set; }

        public SchemaProperties Properties { get; set; }
    }
}
