// ==========================================================================
//  SchemaFieldsReordered.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Schemas
{
    [EventType(nameof(SchemaFieldsReordered))]
    public sealed class SchemaFieldsReordered : SchemaEvent
    {
        public List<long> FieldIds { get; set; }
    }
}
