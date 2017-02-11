// ==========================================================================
//  SchemaUpdated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Events.Schemas
{
    [TypeName("SchemaUpdatedEvent")]
    public class SchemaUpdated : SchemaEvent
    {
        public SchemaProperties Properties { get; set; }
    }
}
