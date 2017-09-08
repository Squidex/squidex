// ==========================================================================
//  SchemaCreated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.CQRS.Events;
using SchemaFields = System.Collections.Generic.List<Squidex.Domain.Apps.Events.Schemas.SchemaCreatedField>;

namespace Squidex.Domain.Apps.Events.Schemas
{
    [EventType(nameof(SchemaCreated))]
    public sealed class SchemaCreated : SchemaEvent
    {
        public string Name { get; set; }

        public SchemaFields Fields { get; set; }

        public SchemaProperties Properties { get; set; }
    }
}
