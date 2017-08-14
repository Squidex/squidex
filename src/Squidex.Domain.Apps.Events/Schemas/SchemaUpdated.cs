// ==========================================================================
//  SchemaUpdated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Schemas
{
    [TypeName("SchemaUpdatedEvent")]
    public sealed class SchemaUpdated : SchemaEvent
    {
        public SchemaProperties Properties { get; set; }
    }
}
