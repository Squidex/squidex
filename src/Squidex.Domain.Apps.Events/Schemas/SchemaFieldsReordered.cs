// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Schemas
{
    [EventType(nameof(SchemaFieldsReordered))]
    public sealed class SchemaFieldsReordered : SchemaEvent
    {
        public NamedId<long> ParentFieldId { get; set; }

        public List<long> FieldIds { get; set; }
    }
}
