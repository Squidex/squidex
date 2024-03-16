// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Schemas
{
    [EventType(nameof(SchemaUIFieldsConfigured))]
    public sealed class SchemaUIFieldsConfigured : SchemaEvent
    {
        public FieldNames? FieldsInLists { get; set; }

        public FieldNames? FieldsInReferences { get; set; }
    }
}
