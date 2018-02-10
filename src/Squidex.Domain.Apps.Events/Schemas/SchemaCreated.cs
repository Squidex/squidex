// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.EventSourcing;
using SchemaFields = System.Collections.Generic.List<Squidex.Domain.Apps.Events.Schemas.SchemaCreatedField>;

namespace Squidex.Domain.Apps.Events.Schemas
{
    [EventType(nameof(SchemaCreated))]
    public sealed class SchemaCreated : SchemaEvent
    {
        public string Name { get; set; }

        public SchemaFields Fields { get; set; }

        public SchemaProperties Properties { get; set; }

        public bool Publish { get; set; }
    }
}
