// ==========================================================================
//  SchemaFieldsReordered.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Schemas
{
    [TypeName("SchemaFieldsReorderedEvent")]
    public sealed class SchemaFieldsReordered : SchemaEvent
    {
        public List<long> FieldIds { get; set; }
    }
}
